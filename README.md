# AppEtiquetado

Aplicación móvil MAUI + Blazor para tabletas. Complemento del sistema POS [Cleeny](../Cleeny), diseñada para agilizar el proceso de etiquetado de productos a granel antes de pasar por caja.

## Flujo de uso

1. Operador busca el producto por nombre o código.
2. Ingresa el peso o cantidad.
3. La app calcula el total (`precio unitario × cantidad`).
4. Presiona **Imprimir etiqueta** → se genera el PDF en Cleeny y se abre el visor del dispositivo para imprimirlo en la etiquetadora (Brother DK-2205).
5. La etiqueta generada queda registrada en Cleeny para trazabilidad.

## Requisitos del servidor

AppEtiquetado se conecta a Cleeny via REST. El servidor debe exponer:

| Método | Ruta | Descripción |
|--------|------|-------------|
| `POST` | `/api/Auth/Login` | Login con `Input.UserName`, `Input.Password` (form-encoded) |
| `GET` | `/api/products?search=&pageSize=20&isActive=true` | Búsqueda de productos (**requiere agregar a Cleeny si no existe**) |
| `POST` | `/api/labels/bulk` | Crear trabajo de etiquetado |
| `GET` | `/api/labels/bulk/{id}/pdf` | Obtener PDF de la etiqueta |

> **Nota:** Si Cleeny no expone `/api/products`, es necesario agregar un `ProductsController` con el endpoint de búsqueda.

## Configuración inicial

Al abrir la app por primera vez, ingresar:
- **URL del servidor**: dirección IP del servidor Cleeny en la red local (ej. `https://192.168.1.10:7155`).
- **Usuario / Contraseña**: credenciales del sistema Cleeny.

La URL se guarda en `Preferences` del dispositivo para sesiones siguientes.

### Acceso desde emulador Android

El emulador Android mapea `10.0.2.2` al `localhost` del host:
```
https://10.0.2.2:7155
```

En builds `DEBUG` se omite la validación del certificado SSL para permitir certificados de desarrollo.

## Build y ejecución

```bash
# Android (emulador o dispositivo)
dotnet build -f net10.0-android

# Windows (escritorio)
dotnet build -f net10.0-windows10.0.19041.0

# iOS
dotnet build -f net10.0-ios
```

## Estructura

```
AppEtiquetado/
├── Components/
│   ├── Layout/
│   │   ├── AppTheme.cs          # Colores y tipografía alineados con Cleeny
│   │   ├── MainLayout.razor     # AppBar con botón de logout
│   │   └── BlankLayout.razor    # Layout vacío para la pantalla de login
│   └── Pages/
│       ├── Login.razor          # Pantalla de inicio de sesión
│       └── Etiquetado.razor     # Pantalla principal de etiquetado
├── Models/                      # DTOs espejo de Cleeny (ProductDto, BulkLabelJobDto, etc.)
└── Services/
    ├── AppApiService.cs         # HttpClient con CookieContainer (auth cookie)
    ├── AuthService.cs           # Login / logout contra Cleeny
    └── LabelService.cs          # Búsqueda de productos y creación de etiquetas
```

## Tecnologías

- .NET 10 MAUI + Blazor Hybrid
- MudBlazor 8.x (mismo stack visual que Cleeny)
- Cookie-based auth (espejo de ASP.NET Core Identity de Cleeny)
