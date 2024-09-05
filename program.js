const net = require('net');
const os = require('os');
const fs = require('fs');
const msgpack = require('@msgpack/msgpack');
const uuidv4 = require('uuid').v4;

class Message {
    Version;
    Id;

    constructor() {
        this.Version = "3.0";
        this.Id = uuidv4();
    }
}

// Derived class Message1
class Message1 extends Message {
    MetaData1;

    constructor(metaData1) {
        super();
        this.MetaData1 = metaData1;
    }
}

// Derived class Message2
class Message2 extends Message {
    MetaData2;

    constructor(metaData2) {
        super();
        this.MetaData2 = metaData2;
    }
}

const message = new Message1("Hi From Message 1");

const PIPE_PATH = os.platform() === 'win32' ? '\\\\.\\pipe\\node_named_pipe' : '/tmp/node_named_pipe';

if (fs.existsSync(PIPE_PATH)) {
    fs.unlinkSync(PIPE_PATH);
}

function writeInt32(socket, value) {
    const buffer = Buffer.alloc(4);
    buffer.writeInt32LE(value, 0);
    console.log('Starting write length');
    socket.write(buffer);
}

// Function to serialize a message with type information
function serializeMessage(message) {
    let type;
    let messageData;

    console.log(message);
    if (message instanceof Message1) {
        type = 0; // Corresponds to [Union(0, typeof(Message1))]
        messageData = { Version: message.Version, Id: message.Id, MetaData1: message.MetaData1 };
    } else if (message instanceof Message2) {
        type = 1; // Corresponds to [Union(1, typeof(Message2))]
        messageData = { MetaData2: message.MetaData2, Version: message.Version, Id: message.Id };
    } else {
        throw new Error("Unknown message type");
    }

    console.log(messageData);
    // Pack the type and the message data together
    const packedMessage = msgpack.encode([type, messageData]);

    return packedMessage;
}

const server = net.createServer((socket) => {
    console.log('Client connected ' + new Date());

    const outBuffer = serializeMessage(message);
	const len = outBuffer.length;

	writeInt32(socket, len);
	socket.write(outBuffer);
    socket.on('end', () => {
        console.log('Client disconnected');
    });
});

server.listen(PIPE_PATH, () => {
    console.log(`Server listening on ${PIPE_PATH}`);
});