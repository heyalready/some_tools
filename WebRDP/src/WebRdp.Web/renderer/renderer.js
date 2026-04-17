class RdpClient {
  constructor() {
    this.sessionId = null;
    this.canvas = document.getElementById('rdp-canvas');
    this.ctx = this.canvas.getContext('2d');
    this.ws = null;
    this.isConnected = false;
  }

  async connect(config) {
    try {
      this.updateStatus('connecting', '正在连接...');
      
      const response = await fetch('http://localhost:5000/api/rdp/connect', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(config)
      });

      if (!response.ok) {
        throw new Error('连接失败');
      }

      const session = await response.json();
      this.sessionId = session.id;
      
      this.connectWebSocket();
      this.setupInputHandlers();
      
      return session;
    } catch (error) {
      console.error('Connection error:', error);
      this.updateStatus('disconnected', '连接失败：' + error.message);
      throw error;
    }
  }

  connectWebSocket() {
    this.ws = new WebSocket('ws://localhost:5001/api/rdp/stream');
    this.ws.binaryType = 'arraybuffer';
    
    this.ws.onopen = () => {
      this.updateStatus('connected', '已连接');
      this.isConnected = true;
    };
    
    this.ws.onmessage = (event) => {
      this.renderFrame(event.data);
    };
    
    this.ws.onerror = (error) => {
      console.error('WebSocket error:', error);
    };
    
    this.ws.onclose = () => {
      if (this.isConnected) {
        this.updateStatus('disconnected', '连接已断开');
        this.isConnected = false;
      }
    };
  }

  renderFrame(data) {
    if (!(data instanceof ArrayBuffer)) {
      return;
    }
    
    const imageData = new Uint8ClampedArray(data);
    const width = Math.sqrt(imageData.length / 4);
    const height = width;
    
    const imgData = new ImageData(imageData, this.canvas.width, this.canvas.height);
    this.ctx.putImageData(imgData, 0, 0);
  }

  setupInputHandlers() {
    this.canvas.addEventListener('keydown', (e) => {
      this.sendInput({ type: 'keyboard', code: e.code, pressed: true });
    });
    
    this.canvas.addEventListener('keyup', (e) => {
      this.sendInput({ type: 'keyboard', code: e.code, pressed: false });
    });
    
    this.canvas.addEventListener('mousedown', (e) => {
      this.sendInput({ 
        type: 'mouse', 
        button: e.button, 
        x: e.offsetX, 
        y: e.offsetY, 
        pressed: true 
      });
    });
    
    this.canvas.addEventListener('mouseup', (e) => {
      this.sendInput({ 
        type: 'mouse', 
        button: e.button, 
        x: e.offsetX, 
        y: e.offsetY, 
        pressed: false 
      });
    });
    
    this.canvas.addEventListener('mousemove', (e) => {
      this.sendInput({ type: 'mouse', x: e.offsetX, y: e.offsetY });
    });
    
    this.canvas.addEventListener('wheel', (e) => {
      e.preventDefault();
      this.sendInput({ 
        type: 'mouse_wheel', 
        delta: e.deltaY,
        x: e.offsetX, 
        y: e.offsetY 
      });
    });
  }

  sendInput(input) {
    if (!this.sessionId) return;
    
    fetch(`http://localhost:5000/api/rdp/input/${this.sessionId}`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(input)
    }).catch(console.error);
  }

  async disconnect() {
    if (this.sessionId) {
      await fetch(`http://localhost:5000/api/rdp/disconnect/${this.sessionId}`, {
        method: 'DELETE'
      });
      this.sessionId = null;
    }
    
    if (this.ws) {
      this.ws.close();
      this.ws = null;
    }
    
    this.isConnected = false;
    this.updateStatus('disconnected', '未连接');
  }

  updateStatus(status, text) {
    const indicator = document.getElementById('status-indicator');
    const statusText = document.getElementById('status-text');
    
    indicator.className = 'status-indicator ' + status;
    statusText.textContent = text;
  }
}

const rdp = new RdpClient();

document.getElementById('btn-login').addEventListener('click', async () => {
  const host = document.getElementById('host').value;
  const port = parseInt(document.getElementById('port').value);
  const username = document.getElementById('username').value;
  const password = document.getElementById('password').value;

  try {
    await rdp.connect({ host, port, username, password });
    
    document.getElementById('login-form').style.display = 'none';
    document.getElementById('rdp-canvas').style.display = 'block';
    document.getElementById('btn-connect').disabled = true;
    document.getElementById('btn-disconnect').disabled = false;
    
    rdp.canvas.focus();
  } catch (error) {
    alert('连接失败：' + error.message);
  }
});

document.getElementById('btn-disconnect').addEventListener('click', async () => {
  await rdp.disconnect();
  
  document.getElementById('login-form').style.display = 'block';
  document.getElementById('rdp-canvas').style.display = 'none';
  document.getElementById('btn-connect').disabled = false;
  document.getElementById('btn-disconnect').disabled = true;
});

document.getElementById('btn-connect').addEventListener('click', () => {
  document.getElementById('login-form').style.display = 'block';
});
