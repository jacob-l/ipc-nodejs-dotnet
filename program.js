const net = require('net');
const os = require('os');
const fs = require('fs');

const PIPE_PATH = os.platform() === 'win32' ? '\\\\.\\pipe\\node_named_pipe' : '/tmp/node_named_pipe';

if (fs.existsSync(PIPE_PATH)) {
    fs.unlinkSync(PIPE_PATH);
}

const server = net.createServer((socket) => {
    console.log('Client connected');
    socket.on('data', (data) => {
        console.log(`Received: ${data}`);
        socket.write(`Echo: ${data}`);
    });

    socket.on('end', () => {
        console.log('Client disconnected');
    });
});

server.listen(PIPE_PATH, () => {
    console.log(`Server listening on ${PIPE_PATH}`);
});