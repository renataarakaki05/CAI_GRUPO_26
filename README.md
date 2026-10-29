# CAI_GRUPO_26 - E-Commerce con Microservicios

Trabajo Práctico de Arquitectura y Diseño de Software.
Sistema de E-Commerce formado por 5 microservicios independientes, cada uno expuesto como una REST API en C# con .NET 10.

Todos los microservicios se construyeron a partir del proyecto base de la cátedra (`MiniApi`): Minimal API, Dapper + SQLite para la persistencia, Serilog para los logs y HealthChecks.

## Integrantes

| Integrante | Microservicios |
|---|---|
| Renata | Products.API, Notifications.API |
| Candela | Users.API |
| Federico | Cart.API, Orders.API |

## Microservicios

| Servicio | Puerto | Descripción | Estado |
|---|---|---|---|
| Products.API | 5101 | Catálogo de productos: alta, baja, modificación y consulta con filtros por categoría y nombre. | Terminado |
| Users.API | 5102 | Registro y login de usuarios, con bloqueo tras 3 intentos fallidos. | Pendiente |
| Orders.API | 5103 | Creación de órdenes validando usuario, productos y stock; cambios de estado. | Pendiente |
| Cart.API | 5104 | Carrito de compras por usuario, validando productos y stock contra Products.API. | Pendiente |
| Notifications.API | 5105 | Registro y envío simulado de notificaciones (Email, Push, SMS) a usuarios existentes. | Terminado |

## Diagrama de arquitectura

```
                        Cliente (Swagger / HTTP)
                                   |
       +-----------+-----------+---+-------+-------------+
       |           |           |           |             |
       v           v           v           v             v
  +---------+ +---------+ +---------+ +---------+ +---------------+
  |Products | |  Users  | | Orders  | |  Cart   | | Notifications |
  |  :5101  | |  :5102  | |  :5103  | |  :5104  | |     :5105     |
  +---------+ +---------+ +---------+ +---------+ +---------------+
  | SQLite  | | SQLite  | | SQLite  | | SQLite  | |    SQLite     |
  +---------+ +---------+ +---------+ +---------+ +---------------+

Llamadas HTTP entre servicios (IHttpClientFactory + X-Correlation-Id):

  Cart.API ----------> Products.API   valida producto y stock
  Orders.API --------> Products.API   valida producto, stock y precio
  Orders.API --------> Users.API      valida usuario
  Notifications.API -> Users.API      valida usuario destinatario
  Products.API ------> Orders.API     revisa órdenes activas antes de eliminar (PRD-004)
```

Cada microservicio tiene su propia base SQLite y no accede a la base de otro servicio.

Comunicación entre servicios:

| Servicio que llama | Servicio llamado | Endpoint | Para qué |
|---|---|---|---|
| Products.API | Orders.API | `GET /api/orders` | No eliminar productos con órdenes Pendiente o Confirmada (PRD-004) |
| Cart.API | Products.API | `GET /api/products/{id}` | Validar que el producto exista y tenga stock (CRT-002, CRT-003) |
| Orders.API | Users.API | `GET /api/users/{id}` | Validar que el usuario exista (ORD-003) |
| Orders.API | Products.API | `GET /api/products/{id}` | Validar productos, stock y tomar el precio unitario (ORD-004, ORD-005) |
| Notifications.API | Users.API | `GET /api/users/{id}` | Validar que el usuario destinatario exista (NTF-001) |

## Cómo ejecutar

### Requisitos

- .NET SDK 10
- Visual Studio Community (o `dotnet` por consola)

### Desde Visual Studio

1. Abrir la solución `MiniApi.slnx`.
2. Para levantar un solo servicio: elegirlo como proyecto de inicio y ejecutar con F5 (perfil `https`, abre Swagger automáticamente).
3. Para levantar varios a la vez: clic derecho en la solución → **Configurar proyectos de inicio** → **Varios proyectos de inicio** → marcar **Iniciar** en cada servicio.

### Desde la consola

Ejecutar cada servicio en una terminal distinta, desde la carpeta raíz del repositorio:

```
dotnet run --project Products.API --launch-profile http
dotnet run --project Users.API --launch-profile http
dotnet run --project Orders.API --launch-profile http
dotnet run --project Cart.API --launch-profile http
dotnet run --project Notifications.API --launch-profile http
```

La base de datos SQLite de cada servicio (por ejemplo `products.db`) se crea sola la primera vez que se ejecuta.

### Orden recomendado

Como algunos servicios dependen de otros, conviene levantarlos en este orden: Users.API → Products.API → Orders.API → Cart.API → Notifications.API.

## Endpoints de soporte (en todos los servicios)

| Endpoint | Descripción |
|---|---|
| `/swagger` | Documentación de la API con ejemplos de request, response y errores |
| `/health` | Estado general del servicio (base de datos + API) |
| `/health/ready` | Indica si el servicio está listo para recibir requests (base de datos) |
| `/health/live` | Indica si el proceso está vivo (API) |
| `/health-ui` | Panel visual de los health checks |

Las respuestas de health checks devuelven el estado `Healthy`, `Degraded` o `Unhealthy`.

## Endpoints de negocio

### Products.API (puerto 5101)

| Método | Endpoint | Descripción | HTTP Status posibles |
|---|---|---|---|
| GET | `/api/products` | Listar productos (filtro `?categoria=` y `?nombre=`) | 200, 500 |
| GET | `/api/products/{id}` | Obtener producto por ID | 200, 404, 500 |
| POST | `/api/products` | Crear nuevo producto | 201, 400, 409, 500 |
| PUT | `/api/products/{id}` | Actualizar producto existente | 200, 400, 404, 500 |
| DELETE | `/api/products/{id}` | Eliminar producto | 204, 404, 409, 500 |

### Users.API (puerto 5102)

| Método | Endpoint | Descripción | HTTP Status posibles |
|---|---|---|---|
| POST | `/api/users/register` | Registrar nuevo usuario | 201, 400, 409, 500 |
| POST | `/api/users/login` | Autenticar usuario (email + password) | 200, 400, 401, 403, 500 |
| GET | `/api/users/{id}` | Obtener usuario por ID (lo usan Orders y Notifications) | 200, 404, 500 |

### Orders.API (puerto 5103)

| Método | Endpoint | Descripción | HTTP Status posibles |
|---|---|---|---|
| GET | `/api/orders` | Listar órdenes (filtro `?usuarioId=`) | 200, 500 |
| GET | `/api/orders/{id}` | Obtener detalle de una orden | 200, 404, 500 |
| POST | `/api/orders` | Crear nueva orden | 201, 400, 404, 409, 422, 500 |
| PUT | `/api/orders/{id}/status` | Actualizar estado de la orden | 200, 400, 404, 409, 500 |

### Cart.API (puerto 5104)

| Método | Endpoint | Descripción | HTTP Status posibles |
|---|---|---|---|
| GET | `/api/cart/{userId}` | Obtener carrito del usuario | 200, 404, 500 |
| POST | `/api/cart/{userId}/items` | Agregar producto al carrito | 200, 400, 404, 422, 500 |
| PUT | `/api/cart/{userId}/items/{productId}` | Actualizar cantidad de un item | 200, 400, 404, 422, 500 |
| DELETE | `/api/cart/{userId}/items/{productId}` | Quitar un producto del carrito | 204, 404, 500 |
| DELETE | `/api/cart/{userId}` | Vaciar carrito completo | 204, 404, 500 |

### Notifications.API (puerto 5105)

| Método | Endpoint | Descripción | HTTP Status posibles |
|---|---|---|---|
| POST | `/api/notifications/send` | Registrar y simular envío de notificación | 201, 400, 404, 500 |
| GET | `/api/notifications/{userId}` | Listar notificaciones de un usuario | 200, 404, 500 |

## Contrato de errores

Todas las respuestas de error (4xx y 5xx) tienen la misma estructura. `errorCode` y `errorMessage` salen del catálogo de cada servicio, y `correlationId` permite encontrar el request en los logs:

```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.4",
  "title": "Not Found",
  "status": 404,
  "detail": "El recurso solicitado no fue encontrado.",
  "instance": "/api/products/99",
  "errorCode": "PRD-001",
  "errorMessage": "Producto no encontrado.",
  "correlationId": "0f8fad5b-d9cb-469f-a165-70867728950e"
}
```

Los errores se manejan con `IExceptionHandler` registrados en `Program.cs` (a través de `ServicesExtensions`), del más específico al genérico:

| Excepción de dominio | Handler | HTTP |
|---|---|---|
| `NotFoundException` | `NotFoundExceptionHandler` | 404 |
| `BusinessRuleException` | `BusinessRuleExceptionHandler` | 409 / 422 |
| `ValidationException` | `ValidationExceptionHandler` | 400 |
| Cualquier otra | `GlobalExceptionHandler` | 500 |

Las respuestas nunca incluyen el stack trace. En `appsettings.Development.json` la opción `ErrorHandling:IncludeExceptionDetails` agrega el mensaje técnico del error (campo `exceptionDetail`); en producción está desactivada.

## Catálogo de códigos de error

### Products.API

| errorCode | HTTP | errorMessage | Cuándo se devuelve |
|---|---|---|---|
| PRD-001 | 404 | Producto no encontrado. | GET/PUT/DELETE cuando el ID no existe. |
| PRD-002 | 400 | Los datos del producto son inválidos. | POST/PUT con campos faltantes o formato incorrecto. |
| PRD-003 | 409 | Ya existe un producto con ese nombre en la categoría. | POST cuando se intenta crear un duplicado. |
| PRD-004 | 409 | El producto tiene órdenes activas y no puede eliminarse. | DELETE cuando hay órdenes Pendiente o Confirmada que lo referencian. |
| PRD-005 | 500 | Error interno al procesar el producto. | Error inesperado en servicio o persistencia. |

### Users.API

| errorCode | HTTP | errorMessage | Cuándo se devuelve |
|---|---|---|---|
| USR-001 | 409 | El email ya está registrado. | POST /register cuando el email ya existe. |
| USR-002 | 400 | Los datos del usuario son inválidos. | POST /register con campos faltantes o formato incorrecto. |
| USR-003 | 401 | Credenciales incorrectas. | POST /login cuando email o contraseña no coinciden. |
| USR-004 | 403 | Usuario bloqueado por demasiados intentos fallidos. | POST /login con 3+ intentos fallidos acumulados. |
| USR-005 | 403 | Usuario bloqueado por detección de fraude. | POST /login cuando el usuario fue bloqueado manualmente. |
| USR-006 | 500 | Error interno al procesar el usuario. | Error inesperado en servicio o persistencia. |

### Orders.API

| errorCode | HTTP | errorMessage | Cuándo se devuelve |
|---|---|---|---|
| ORD-001 | 404 | Orden no encontrada. | GET/PUT cuando el ID de orden no existe. |
| ORD-002 | 400 | Los datos de la orden son inválidos. | POST con campos faltantes o lista de items vacía. |
| ORD-003 | 404 | Usuario no encontrado al crear la orden. | POST cuando el UsuarioId no existe en Users.API. |
| ORD-004 | 404 | Producto no encontrado al crear la orden. | POST cuando algún ProductoId no existe en Products.API. |
| ORD-005 | 422 | Stock insuficiente para uno o más productos. | POST cuando la cantidad solicitada supera el stock disponible. |
| ORD-006 | 409 | El estado de la orden no puede ser modificado. | PUT /status cuando la transición de estado no es válida. |
| ORD-007 | 500 | Error interno al procesar la orden. | Error inesperado en servicio o persistencia. |

### Cart.API

| errorCode | HTTP | errorMessage | Cuándo se devuelve |
|---|---|---|---|
| CRT-001 | 404 | Carrito no encontrado. | GET/PUT/DELETE cuando el userId no tiene carrito activo. |
| CRT-002 | 404 | Producto no encontrado. | POST/PUT cuando el ProductoId no existe en Products.API. |
| CRT-003 | 422 | Stock insuficiente para agregar al carrito. | POST/PUT cuando la cantidad supera el stock disponible. |
| CRT-004 | 400 | Cantidad inválida. | POST/PUT cuando la cantidad es menor o igual a cero. |
| CRT-005 | 500 | Error interno al procesar el carrito. | Error inesperado en servicio o persistencia. |

### Notifications.API

| errorCode | HTTP | errorMessage | Cuándo se devuelve |
|---|---|---|---|
| NTF-001 | 404 | Usuario no encontrado. | POST cuando el UsuarioId no existe en Users.API. |
| NTF-002 | 400 | Los datos de la notificación son inválidos. | POST con campos faltantes o tipo no reconocido. |
| NTF-003 | 404 | No se encontraron notificaciones para el usuario. | GET cuando el userId no tiene notificaciones registradas. |
| NTF-004 | 500 | Error interno al procesar la notificación. | Error inesperado en servicio o persistencia. |

## Logs (Serilog)

Cada servicio escribe sus logs en la carpeta `logs/` de su proyecto:

| Destino | Formato | Contenido |
|---|---|---|
| Consola | Texto legible | Todos los logs, con servicio, Correlation ID y endpoint |
| `logs/<servicio>.json` | JSON estructurado | Timestamp, nivel, servicio, endpoint, Correlation ID y errorCode cuando aplica |
| `logs/audit.log` | Texto | Una línea por request: fecha, Correlation ID, método, ruta, status y duración |

- Inicio y fin de cada request con su duración.
- Errores de negocio (4xx) como `Warning`; errores inesperados (500) como `Error`.

### Correlation ID

Cada request recibe un `X-Correlation-Id` único (o reutiliza el que llega en el header). Se agrega a todos los logs del request, se devuelve en el header de la respuesta y en el campo `correlationId` de los errores, y se propaga en las llamadas HTTP a otros servicios.

## Estructura de cada microservicio

Todos los servicios siguen la misma estructura, tomada del `MiniApi` de la cátedra:

```
Products.API/
  ├── Clients/             # Llamadas HTTP a otros microservicios
  ├── Data/                # DatabaseInitializer y repositorio (Dapper + SQLite)
  ├── DTOs/                # Request y Response DTOs
  ├── Exceptions/          # ErrorCodes, NotFoundException, BusinessRuleException, ValidationException
  ├── ExceptionHandlers/   # Un IExceptionHandler por tipo de excepción
  ├── Extensions/          # Logging, servicios, middleware y endpoints
  │   └── Endpoints/       # Definición de los endpoints (Minimal API)
  ├── HealthChecks/        # SqliteHealthCheck y ApiStatusCheck
  ├── Middleware/          # AuditMiddleware y CorrelationIdMiddleware
  ├── Models/              # Entidades del dominio
  ├── Services/            # Lógica de negocio
  ├── Swagger/             # Ejemplos de error para Swagger
  ├── logs/                # Archivos de log de Serilog
  └── Program.cs
```

## Capturas de Swagger

Capturas de Swagger UI mostrando ejemplos de response de error con `errorCode` y `errorMessage` (se guardan en la carpeta `docs/`):

| Servicio | Captura |
|---|---|
| Products.API | Pendiente |
| Users.API | Pendiente |
| Orders.API | Pendiente |
| Cart.API | Pendiente |
| Notifications.API | Pendiente |
