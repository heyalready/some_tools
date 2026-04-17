const { app, BrowserWindow, ipcMain } = require('electron');
const path = require('path');
const { spawn } = require('child_process');

let mainWindow;
let rdpServiceProcess;

async function startRdpService() {
  const isDev = process.env.NODE_ENV === 'development';
  const servicePath = isDev 
    ? path.join(__dirname, '../WebRdp.Service/bin/Debug/net6.0/WebRdp.Service')
    : path.join(process.resourcesPath, 'WebRdp.Service');

  rdpServiceProcess = spawn(servicePath, {
    cwd: path.dirname(servicePath),
    env: { ...process.env, ASPNETCORE_ENVIRONMENT: isDev ? 'Development' : 'Production' }
  });

  rdpServiceProcess.stdout.on('data', (data) => {
    console.log(`RDP Service: ${data}`);
  });

  rdpServiceProcess.stderr.on('data', (data) => {
    console.error(`RDP Service Error: ${data}`);
  });

  await new Promise(resolve => setTimeout(resolve, 3000));
}

function createWindow() {
  mainWindow = new BrowserWindow({
    width: 1400,
    height: 900,
    webPreferences: {
      preload: path.join(__dirname, 'preload.js'),
      contextIsolation: true,
      nodeIntegration: false
    }
  });

  const startUrl = process.env.ELECTRON_START_URL || `file://${path.join(__dirname, '../dist/index.html')}`;
  mainWindow.loadURL(startUrl);
  
  if (process.env.NODE_ENV === 'development') {
    mainWindow.webContents.openDevTools();
  }
}

app.whenReady().then(async () => {
  await startRdpService();
  createWindow();
});

app.on('before-quit', () => {
  if (rdpServiceProcess) {
    rdpServiceProcess.kill();
  }
});

app.on('window-all-closed', () => {
  if (process.platform !== 'darwin') {
    app.quit();
  }
});

ipcMain.handle('rdp-connect', async (event, config) => {
  const response = await fetch('http://localhost:5000/api/rdp/connect', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(config)
  });
  return response.json();
});

ipcMain.handle('rdp-disconnect', async (event, sessionId) => {
  await fetch(`http://localhost:5000/api/rdp/disconnect/${sessionId}`, {
    method: 'DELETE'
  });
  return { success: true };
});
