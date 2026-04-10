# 🏥 MediApp - Sistema de Gestión de Citas Médicas

<p align="center">
  <img src="https://img.shields.io/badge/.NET-10.0-purple?style=for-the-badge&logo=.net" alt=".NET 10">
  <img src="https://img.shields.io/badge/Blazor-Interactive-blue?style=for-the-badge&logo=blazor" alt="Blazor">
  <img src="https://img.shields.io/badge/JWT-Authentication-green?style=for-the-badge&logo=json" alt="JWT">
</p>

---

## 📋 Descripción

**MediApp** es una aplicación web para la gestión de citas médicas desarrollada con **.NET 10** y **Razor Components interactivos**. Permite a pacientes registrarse, iniciar sesión, gestionar citas, ver historias clínicas, generar recetas y registrar pagos.

### ✨ Características Principales

| Módulo | Descripción |
|--------|-------------|
| 🔐 Autenticación | Registro e inicio de sesión con JWT |
| 📅 Citas | Reserva, actualización y cancelación de citas |
| 👨‍⚕️ Doctores | Gestión de doctores (solo Admin) |
| 📋 Historia Clínica | Registro y consulta de historias clínicas |
| 💊 Recetas | Gestión y descarga de recetas |
| 💳 Pagos | Registro de pagos por citas |

---

## 🏗️ Arquitectura del Proyecto

```
MediApp/
├── docker-compose.yml          # PostgreSQL + pgAdmin (opcional)
├── MediApp.slnx               # Solución .NET
└── src/
    ├── MediApp.Domain/         # Entidades, Enums, Interfaces
    ├── MediApp.Application/    # Servicios y lógica de aplicación
    ├── MediApp.Infrastructure/ # Repositorios y EF Core
    ├── MediApp.Api/           # API Minimal (.NET 10)
    └── MediApp.Blazor/        # Frontend Razor Components interactivos
```

---

## 👥 Roles de Usuario

| Rol | Permisos |
|-----|----------|
| Paciente | Registrarse, iniciar sesión, ver/agendar/cancelar citas, historial y recetas |
| Doctor | Ver citas asignadas, crear historia clínica y recetas |
| Admin | Gestionar doctores, ver citas y pagos |

---

## 🛠️ Tecnologías Utilizadas

| Tecnología | Uso |
|------------|-----|
| .NET 10 | Backend y frontend |
| ASP.NET Core Minimal API | Endpoints REST |
| Razor Components interactivos | UI del cliente |
| Entity Framework Core | Acceso a datos |
| InMemoryDatabase | Persistencia en desarrollo local |
| JWT | Autenticación |
| BCrypt | Hash de contraseñas |

---

## 🚀 Instalación y Ejecución

### Prerrequisitos

- .NET 10 SDK

> Nota: El proyecto actual usa una base de datos en memoria durante el desarrollo local. El `docker-compose.yml` está disponible para PostgreSQL/pgAdmin, pero no es necesario para ejecutar la app actualmente.

### 1. Ejecutar la API

```bash
cd src/MediApp.Api
dotnet run
```

La API se ejecuta por defecto en `http://localhost:5004`.

### 2. Ejecutar el Frontend

```bash
cd src/MediApp.Blazor
dotnet run
```

El frontend se ejecuta por defecto en `http://localhost:5128`.

---

## 🔑 Credenciales por Defecto

| Rol | Email | Contraseña |
|-----|-------|------------|
| Admin | `admin@mediapp.com` | `admin123` |

> Los pacientes pueden registrarse en la UI. Los doctores se crean desde el backend con un usuario Admin.

---

## 📡 Endpoints de la API

### Autenticación

| Método | Endpoint | Descripción |
|--------|----------|-------------|
| POST | `/api/auth/register` | Registrar nuevo paciente |
| POST | `/api/auth/login` | Iniciar sesión |
| GET | `/api/auth/me` | Obtener datos del usuario actual (requiere token) |

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

## 📄 Rutas de la UI

- `/login` — Iniciar sesión
- `/register` — Registrarse

---

## 📝 Notas de Desarrollo

- El frontend usa `ApiService` y `AuthService` para consumir la API en `http://localhost:5004`.
- La autenticación usa JWT y el token se aplica automáticamente en los headers de `HttpClient`.
- Los datos se almacenan en memoria y se reinician al cerrar la aplicación.
- El proyecto actual no depende de PostgreSQL para el desarrollo local.

---

## 📄 Licencia

Este proyecto es de uso educativo y demostrativo.

---

<p align="center">
  ❤️ Desarrollado con .NET 10 y Blazor
</p>
