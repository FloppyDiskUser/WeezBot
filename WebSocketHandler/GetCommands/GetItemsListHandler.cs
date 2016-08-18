﻿using System.Threading.Tasks;
using PoGo.NecroBot.CLI.WebSocketHandler.GetCommands.Tasks;
using PoGo.NecroBot.Logic.State;
using SuperSocket.WebSocket;

namespace PoGo.NecroBot.CLI.WebSocketHandler.GetCommands
{
    class GetItemsListHandler : IWebSocketRequestHandler
    {
        public string Command { get; private set; }

        public GetItemsListHandler()
        {
            Command = "GetItemsList";
        }

        public async Task Handle(ISession session, WebSocketSession webSocketSession, dynamic message)
        {
            await GetItemListTask.Execute(session, webSocketSession, (string)message.RequestID);
        }

    }
}
