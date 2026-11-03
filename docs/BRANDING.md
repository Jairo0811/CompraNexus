# Identidad visual oficial de CompraNexus

Los recursos visuales del proyecto se sirven desde `frontend/public/` y no alteran la base de datos ni la API.

## Archivos requeridos

| Archivo | Uso |
|---|---|
| `frontend/public/compranexus-logo.png` | Logotipo completo en login |
| `frontend/public/compranexus-isotipo.png` | Icono compacto en barra lateral |
| `frontend/public/favicon.ico` | Icono de pestaña |

Estos tres archivos PNG/ICO fueron suministrados fuera de GitHub: **deben añadirse al control de versiones antes de fusionar este PR**. Los cambios de React y CSS no incluyen los bytes de imagen.

## Integración

- `App.tsx`: el isotipo oficial sustituye el ícono genérico `Network`, conservando el nombre CompraNexus en el sidebar.
- `Login.tsx`: el logotipo completo sustituye el icono y el texto duplicado de la cabecera. Se conserva el mensaje académico de bienvenida y el candado del formulario como iconografía funcional.
- `index.html`: usa `/favicon.ico` en vez del favicon SVG de demostración.
- `styles.css`: estilos responsivos con proporción original y `object-fit: contain`.

La prueba Playwright valida que ambas imágenes se carguen con tamaño real y que el favicon responda como imagen.

## Paso local (PowerShell 5.1)

Tras cambiar a la rama `feature/branding-oficial`, verificar:

```powershell
Test-Path .\frontend\public\compranexus-logo.png
Test-Path .\frontend\public\compranexus-isotipo.png
Test-Path .\frontend\public\favicon.ico

git add -- frontend/public/compranexus-logo.png frontend/public/compranexus-isotipo.png frontend/public/favicon.ico
git commit -m "assets: agregar identidad visual oficial CompraNexus"
git push origin feature/branding-oficial
```

Si alguno de los tres `Test-Path` falla, primero corregir su ubicación o nombre. No eliminar ni recrear volúmenes de Docker. Una vez en verde el CI, fusionar el PR y ejecutar `docker compose up --build -d frontend` con el `main` actualizado.
