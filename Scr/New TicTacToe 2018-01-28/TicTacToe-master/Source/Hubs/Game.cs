﻿namespace TicTacToe.Hubs
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.AspNet.SignalR;
    using Microsoft.AspNet.SignalR.Hubs;
    using Logic;

    [HubName("TicTacToe")]
    public class Game : Hub
    {
        private static int gamesPlayed = 0;

        private static readonly object syncRoot = new object();
        private static readonly List<Client> clients = new List<Client>();
        private static readonly List<TicTacToe> games = new List<TicTacToe>();
        private static readonly Random random = new Random();

        public override Task OnConnected()
        {
            return this.SendStatsUpdate();
        }

        public Task OnDisconnected()
        {
            var game = games.FirstOrDefault( 
                g => g.PlayerOne.ConnectionId == Context.ConnectionId || g.PlayerTwo.ConnectionId == Context.ConnectionId
            );

            if (game == null)
            {
                var clientWithoutGame = clients.FirstOrDefault(x => x.ConnectionId == Context.ConnectionId);

                if (clientWithoutGame != null)
                {
                    clients.Remove(clientWithoutGame);

                    SendStatsUpdate();
                }

                return null;
            }

            if (game != null)
            {
                games.Remove(game);
            }

            var client = game.PlayerOne.ConnectionId == Context.ConnectionId ? game.PlayerOne : game.PlayerTwo;

            if (client == null)
            {
                return null;
            }

            clients.Remove(client);

            if (client.Opponent != null)
            {
                SendStatsUpdate();

                return Clients.Client(client.Opponent.ConnectionId).opponentDisconnected(client.Name);
            }

            return null;
        }

        private Task SendStatsUpdate()
        {
            return Clients.All.refreshAmountOfPlayers(new
            {
                totalGamesPlayed = gamesPlayed,
                amountOfGames = games.Count,
                amountOfClients = clients.Count
            });
        }

        public void RegisterClient(string data)
        {
            lock (syncRoot)
            {
                var client = clients.FirstOrDefault(x => x.ConnectionId == Context.ConnectionId);

                if (client == null)
                {
                    client = new Client
                    {
                        ConnectionId = Context.ConnectionId,
                        Name = data
                    };

                    clients.Add(client);
                }

                client.IsPlaying = false;
            }

            this.SendStatsUpdate();

            this.Clients.Client(Context.ConnectionId).registerComplete();
        }

        public void Play(int position)
        {
            var game = games.FirstOrDefault(
                g => g.PlayerOne.ConnectionId == Context.ConnectionId || 
                g.PlayerTwo.ConnectionId == Context.ConnectionId
            );

            if (game == null || game.IsGameOver)
            {
                return;
            }

            int marker = 0;

            if (game.PlayerTwo.ConnectionId == Context.ConnectionId)
            {
                marker = 1;
            }

            var player = marker == 0 ? game.PlayerOne : game.PlayerTwo;

            if (player.WaitingForMove)
            {
                return;
            }

            Clients.Client(game.PlayerOne.ConnectionId).addMarkerPlacement(new GameInformation
            {
                OpponentName = player.Name,
                MarkerPosition = position
            });

            Clients.Client(game.PlayerTwo.ConnectionId).addMarkerPlacement(new GameInformation
            {
                OpponentName = player.Name,
                MarkerPosition = position
            });

            if(game.Play(marker, position))
            {
                games.Remove(game);
                gamesPlayed += 1;

                Clients.Client(game.PlayerOne.ConnectionId).gameOver(player.Name);
                Clients.Client(game.PlayerTwo.ConnectionId).gameOver(player.Name);
            }

            if (game.IsGameOver && game.IsDraw)
            {
                games.Remove(game);
                gamesPlayed += 1;

                Clients.Client(game.PlayerOne.ConnectionId).gameOver("It's a draw!");
                Clients.Client(game.PlayerTwo.ConnectionId).gameOver("It's a draw!");
            }

            if (!game.IsGameOver)
            {
                player.WaitingForMove = !player.WaitingForMove;
                player.Opponent.WaitingForMove = !player.Opponent.WaitingForMove;

                Clients.Client(player.Opponent.ConnectionId).waitingForMarkerPlacement(player.Name);
            }

            this.SendStatsUpdate();
        }

        public void FindOpponent()
        {
            var player = clients.FirstOrDefault(x => x.ConnectionId == Context.ConnectionId);

            if (player == null)
            {
                return;

            }

            player.LookingForOpponent = true;

            var opponent = clients
                .Where(x => x.ConnectionId != Context.ConnectionId && x.LookingForOpponent && !x.IsPlaying)
                .OrderBy(x => Guid.NewGuid())
                .FirstOrDefault();

            if (opponent == null)
            {
                Clients.Client(Context.ConnectionId).noOpponents(); 

                return;
            }

            player.IsPlaying = true;
            player.LookingForOpponent = false;
            opponent.IsPlaying = true;
            opponent.LookingForOpponent = false;

            player.Opponent = opponent;
            opponent.Opponent = player;

            Clients.Client(Context.ConnectionId).foundOpponent(opponent.Name);
            Clients.Client(opponent.ConnectionId).foundOpponent(player.Name);

            if (random.Next(0, 5000) % 2 == 0)
            {
                player.WaitingForMove = false;
                opponent.WaitingForMove = true;

                Clients.Client(player.ConnectionId).waitingForMarkerPlacement(opponent.Name);
                Clients.Client(opponent.ConnectionId).waitingForOpponent(opponent.Name);
            }
            else
            {
                player.WaitingForMove = true;
                opponent.WaitingForMove = false;

                Clients.Client(opponent.ConnectionId).waitingForMarkerPlacement(opponent.Name);
                Clients.Client(player.ConnectionId).waitingForOpponent(opponent.Name);
            }

            lock (syncRoot)
            {
                games.Add(new TicTacToe {
                    PlayerOne = player,
                    PlayerTwo = opponent
                });
            }

            SendStatsUpdate();
        }
    }
}