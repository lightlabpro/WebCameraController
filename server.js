// Simple Express + WebSocket relay server
const express = require("express");
const http = require("http");
const path = require("path");
const WebSocket = require("ws");

const PORT = 8080;

const app = express();
app.use(express.static(path.join(__dirname, "public"))); // serves controller.html

const server = http.createServer(app);
const wss = new WebSocket.Server({ server, path: "/ws" });

wss.on("connection", (ws, req) => {
  const ip = req.socket.remoteAddress;
  console.log(`[WS] Client connected: ${ip}`);

  ws.on("message", (data) => {
    // Broadcast orientation messages to everyone (Unity will listen)
    for (const client of wss.clients) {
      if (client.readyState === WebSocket.OPEN) {
        client.send(data);
      }
    }
  });

  ws.on("close", () => console.log(`[WS] Client disconnected: ${ip}`));
});

server.listen(PORT, () => {
  console.log(`\nServer running on http://0.0.0.0:${PORT}`);
  console.log(`Open this on your phone:   http://<your-computer-LAN-IP>:${PORT}/controller.html`);
});
