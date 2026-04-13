# 🏥 MediApp - Sistema de Gestión de Citas Médicas

<p align="center">
  <img src="https://img.shields.io/badge/.NET-10.0-purple?style=for-the-badge&logo=.net" alt=".NET 10">
  <img src="https://img.shields.io/badge/Blazor-Interactive-blue?style=for-the-badge&logo=blazor" alt="Blazor">
  <img src="https://img.shields.io/badge/JWT-Authentication-green?style=for-the-badge&logo=json" alt="JWT">
</p>

---

## 📋 Descripción

**MediApp** es una aplicación web para la gestión de citas médicas desarrollada con **.NET 10** y **Blazor Server**. Permite a pacientes registrarse, iniciar sesión, gestionar citas, ver historias clínicas, generar recetas y registrar pagos.

### ✨ Características Principales

| Módulo | Descripción |
|--------|-------------|
| 🔐 Autenticación | Registro e inicio de sesión con JWT |
| 📅 Citas | Reserva, actualización y cancelación de citas |
| 👨‍⚕️ Doctores | Gestión de doctores (solo Admin) |
| 👥 Pacientes | Gestión de pacientes (Admin puede agregar) |
| 📋 Historia Clínica | Registro y consulta de historias clínicas |
| 💊 Recetas | Gestión y descarga de recetas |
| 💳 Pagos | Registro de pagos por citas |
| 📊 Dashboard | Estadísticas para Admin y Doctor |

---

## 🏗️ Arquitectura del Proyecto

El proyecto está separado en dos soluciones independientes:

```
MediApp/
├── MediApp-Api/                    # Backend (API)
│   ├── MediApp-Api.slnx           # Solución API
│   └── src/
│       ├── MediApp.Domain/         # Entidades, Enums, Interfaces
│       ├── MediApp.Application/    # Servicios y lógica de aplicación
│       ├── MediApp.Infrastructure/ # Repositorios y EF Core
│       └── MediApp.Api/           # API Minimal (.NET 10)
│
├── MediApp-Web/                    # Frontend (Blazor)
│   ├── MediApp-Web.slnx           # Solución Web
│   └── src/
│       └── MediApp.Blazor/        # Frontend Blazor Server
│
├── docker-compose.yml              # PostgreSQL + pgAdmin (opcional)
└── README.md
```

---

## 👥 Roles de Usuario

| Rol | Permisos |
|-----|----------|
| Paciente | Registrarse, iniciar sesión, ver/agendar/cancelar citas, historial y recetas |
| Doctor | Ver sus citas, ver sus pacientes atendidos, crear historia clínica y recetas |
| Admin | Dashboard con estadísticas, gestionar doctores, gestionar pacientes, ver todas las citas y pagos |

---

## 🛠️ Tecnologías Utilizadas

| Tecnología | Uso |
|------------|-----|
| .NET 10 | Backend y frontend |
| ASP.NET Core Minimal API | Endpoints REST |
| Blazor Server | UI del cliente |
| Entity Framework Core | Acceso a datos |
| InMemoryDatabase | Persistencia en desarrollo local |
| JWT | Autenticación |
| BCrypt | Hash de contraseñas |
| CORS | Configuración de permisos cross-origin |

---

## 🚀 Instalación y Ejecución

### Prerrequisitos

- .NET 10 SDK

### 1. Ejecutar la API

```bash
cd MediApp-Api/src/MediApp.Api
dotnet run --urls "http://localhost:5004"
```

### 2. Ejecutar el Frontend

```bash
cd MediApp-Web/src/MediApp.Blazor
dotnet run --urls "http://localhost:5000"
```

### Acceder a la aplicación

- **Frontend**: http://localhost:5000
- **Swagger API**: http://localhost:5004/swagger/index.html

---

## 🔑 Credenciales por Defecto

Al iniciar la API se crean automáticamente:

| Rol | Email | Contraseña |
|-----|-------|------------|
| Admin | `admin@mediapp.com` | `admin123` |
| Doctor | `juan.perez@mediapp.com` | `doctor123` |
| Paciente | `roberto.aguilar@email.com` | `paciente123` |

> Se crean 10 doctores y 10 pacientes automáticamente al iniciar la API.

---

## 📡 Endpoints de la API

### Autenticación

| Método | Endpoint | Descripción |
|--------|----------|-------------|
| POST | `/api/auth/register` | Registrar nuevo paciente |
| POST | `/api/auth/login` | Iniciar sesión |
| GET | `/api/auth/me` | Obtener datos del usuario actual (requiere token) |
| GET | `/api/auth/dashboard` | Estadísticas del dashboard (Admin) |
| GET | `/api/auth/pacientes-admin` | Listar todos los pacientes (Admin) |
| GET | `/api/auth/pacientes-doctor` | Listar pacientes atendidos (Doctor) |
| GET | `/api/auth/doctores` | Listar doctores (Admin) |

### Doctores

| Método | Endpoint | Descripción |
|--------|----------|-------------|
| GET | `/api/doctores/` | Listar doctores activos |
| GET | `/api/doctores/{id}` | Obtener un doctor |
| POST | `/api/doctores/` | Crear doctor (Admin) |
| PUT | `/api/doctores/{id}` | Actualizar doctor (Admin) |
| DELETE | `/api/doctores/{id}` | Desactivar doctor (Admin) |
| PUT | `/api/doctores/{id}/horario` | Actualizar horario de doctor |

### Citas

| Método | Endpoint | Descripción |
|--------|----------|-------------|
| GET | `/api/citas` | Listar citas |
| GET | `/api/citas/{id}` | Obtener cita |
| POST | `/api/citas` | Crear cita |
| PUT | `/api/citas/{id}` | Actualizar cita |
| DELETE | `/api/citas/{id}` | Cancelar cita |
| GET | `/api/citas/disponibilidad` | Consultar disponibilidad |
| GET | `/api/citas/paciente/{pacienteId}` | Citas de un paciente |
| GET | `/api/citas/doctor/{doctorId}` | Citas de un doctor |

### Historia Clínica

| Método | Endpoint | Descripción |
|--------|----------|-------------|
| GET | `/api/historias/paciente/{pacienteId}` | Historial de paciente |
| GET | `/api/historias/{id}` | Obtener historia clínica |
| POST | `/api/historias` | Crear historia clínica |
| PUT | `/api/historias/{id}` | Actualizar historia clínica |

### Recetas

| Método | Endpoint | Descripción |
|--------|----------|-------------|
| GET | `/api/recetas/paciente/{pacienteId}` | Recetas de paciente |
| GET | `/api/recetas/{id}` | Obtener receta |
| POST | `/api/recetas` | Crear receta |
| GET | `/api/recetas/{id}/pdf` | Descargar receta en PDF |

### Pagos

| Método | Endpoint | Descripción |
|--------|----------|-------------|
| GET | `/api/pagos` | Listar pagos |
| GET | `/api/pagos/cita/{citaId}` | Pago de una cita |
| POST | `/api/pagos` | Registrar pago |

---

## 📄 Rutas de la UI (Blazor)

- `/` — Dashboard (según rol del usuario)
- `/login` — Iniciar sesión
- `/register` — Registrarse
- `/citas` — Mis citas (Paciente)
- `/reservar` — Reservar cita (Paciente)
- `/historial` — Historial médico (Paciente)
- `/admin/doctores` — Gestionar doctores (Admin)
- `/admin/pacientes` — Ver y agregar pacientes (Admin)
- `/admin/citas` — Todas las citas (Admin)
- `/admin/pagos` — Ver pagos (Admin)
- `/doctor/citas` — Citas del día (Doctor)
- `/doctor/pacientes` — Mis pacientes (Doctor)

---

## 📝 Notas de Desarrollo

- El proyecto está separado en dos carpetas: `MediApp-Api` y `MediApp-Web`
- El frontend Blazor consume la API en `http://localhost:5004` via HttpClient
- La autenticación usa JWT y el token se aplica automáticamente en los headers
- Los datos se almacenan en memoria y se reinician al cerrar la aplicación
- CORS está configurado para permitir cualquier origen en desarrollo
- El proyecto actual no depende de PostgreSQL para el desarrollo local

---

## 📄 Licencia

Este proyecto es de uso educativo y demostrativo.

---

<p align="center">
  ❤️ Desarrollado con .NET 10 y Blazor
</p>