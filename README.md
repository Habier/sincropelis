# SincroPelis

Reproduce ficheros de vídeo sincronizados con tus amigos.

## Características

- Reproducción de video local (MP4, MKV, AVI, etc.)
- Sincronización en tiempo real entre maestro y clientes
- Controles: play/pause, seek ±5 segundos, volumen
- Subtítulos y audio múltiple
- Modo pantalla completa
- Atajos de teclado:
  - `Espacio` - Play/Pause
  - `Flecha izquierda/derecha` - Retroceder/Avanzar 5 segundos
  - `Flecha arriba/abajo` - Subir/Bajar volumen
  - `ESC` - Salir de pantalla completa

## Requisitos

- Windows 10/11 (x64)

## Cómo Usar

### Modo Maestro (servidor)
1. Ejecuta `SincroPelis.exe`
2. Selecciona un archivo de video
3. Desmarca "Maestro" (o márcalo si quieres ser maestro)
4. Haz click en "Iniciar"
5. Asegúrate de tener abierto el puerto TCP configurado (por defecto, `9000`) en el firewall/router para que otros puedan conectarse.

### Modo Cliente
1. Ejecuta `SincroPelis.exe`
2. Ingresa la IP del maestro
3. Ingresa el puerto (default: 9000)
4. Click en "Conectar"

La reproducción se sincronizará automáticamente.

## Desarrollo

### Build

```bash
# Build de desarrollo
dotnet build SincroPelis/SincroPelis.csproj

# Build de release (genera .exe)
dotnet publish SincroPelis/SincroPelis.csproj -c Release
```

### Build con reducción de plugins y ZIP

```powershell
powershell -ExecutionPolicy Bypass -File build-release.ps1
```

## Licencia

MIT
