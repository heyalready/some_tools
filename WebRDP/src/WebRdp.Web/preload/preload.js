const { contextBridge, ipcRenderer } = require('electron');

contextBridge.exposeInMainWorld('rdpAPI', {
  connect: (config) => ipcRenderer.invoke('rdp-connect', config),
  disconnect: (sessionId) => ipcRenderer.invoke('rdp-disconnect', sessionId)
});
