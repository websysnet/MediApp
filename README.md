# 🏥 MediApp - Sistema de Gestión de Citas Médicas

<p align="center">
  <img src="https://img.shields.io/badge/.NET-10.0-purple?style=for-the-badge&logo=.net" alt=".NET 10">
  <img src="https://img.shields.io/badge/Blazor-WebAssembly-blue?style=for-the-badge&logo=blazor" alt="Blazor">
  <img src="https://img.shields.io/badge/PostgreSQL-16-336791?style=for-the-badge&logo=postgresql" alt="PostgreSQL">
  <img src="https://img.shields.io/badge/JWT-Authentication-green?style=for-the-badge&logo=json" alt="JWT">
</p>

---

## 📋 Descripción

**MediApp** es una aplicación web completa para la gestión de citas médicas, desarrollada con **.NET 10** y **Blazor Web**. Permite a pacientes reservar citas, a doctores gestionar sus horarios y audiencias, y a administradores gestionar el sistema completo.

### ✨ Características Principales

| Módulo | Descripción |
|--------|-------------|
| 🔐 **Autenticación** | Registro e inicio de sesión con JWT |
| 📅 **Citas** | Reserva, cancelación y gestión de citas |
| 👨‍⚕️ **Doctores** | Gestión de doctores (solo Admin) |
| 📋 **Historia Clínica** | Registro de diagnósticos y observaciones |
| 💊 **Recetas** | Creación de recetas médicas con PDF |
| 💳 **Pagos** | Registro de pagos por consultas |

---

## 🏗️ Arquitectura del Proyecto

```
MediApp/
├── docker-compose.yml          # PostgreSQL + pgAdmin
├── MediApp.sln                # Solución .NET
└── src/
    ├── MediApp.Domain/         # Entidades, Enums, Interfaces
    ├── MediApp.Application/    # Servicios (reservado)
    ├── MediApp.Infrastructure/ # EF Core, Repositorios
    ├── MediApp.Api/           # API Minimal (.NET 10)
    └── MediApp.Blazor/         # Frontend Blazor Web
```

---

## 👥 Roles de Usuario

| Rol | Permisos |
|-----|----------|
| 🟢 **Paciente** | Registrarse, reservar/cancelar citas, ver historial, descargar recetas |
| 🔵 **Doctor** | Ver citas, crear historia clínica y recetas |
| 🔴 **Admin** | Gestionar doctores, ver todas las citas y pagos |

---

## 🛠️ Tecnologías Utilizadas

| Tecnología | Versión | Uso |
|------------|---------|-----|
| **.NET** | 10.0 | Runtime |
| **ASP.NET Core** | 10.0 | API Minimal |
| **Blazor** | 10.0 | Frontend Web |
| **Entity Framework Core** | 10.0 | ORM |
| **PostgreSQL** | 16 | Base de datos |
| **JWT** | - | Autenticación |
| **BCrypt** | 4.0.3 | Hash de contraseñas |
| **QuestPDF** | 2024.10.2 | Generación de PDFs |

---

## 🚀 Instalación y Ejecución

### Prerrequisitos

- .NET 10 SDK
- Docker (para PostgreSQL)

### 1. Iniciar PostgreSQL

```bash
cd MediApp
docker compose up -d
```

### 2. Ejecutar la API

```bash
cd src/MediApp.Api
dotnet run
```

La API estará disponible en: `http://localhost:5004`

### 3. Ejecutar el Frontend

```bash
cd src/MediApp.Blazor
dotnet run
```

El frontend estará disponible en: `http://localhost:5005`

---

## 🔑 Credenciales por Defecto

| Rol | Email | Contraseña |
|-----|-------|------------|
| Admin | `admin@mediapp.com` | `admin123` |

> **Nota:** Los pacientes pueden registrarse desde la página de registro. Los doctores deben ser creados por un Administrador.

---

## 📡 Endpoints de la API

### 🔐 Autenticación
| Método | Endpoint | Descripción |
|--------|----------|-------------|
| POST | `/api/auth/register` | Registrar nuevo paciente |
| POST | `/api/auth/login` | Iniciar sesión |
| GET | `/api/auth/me` | Obtener usuario actual |

### 👨‍⚕️ Doctores (Admin)
| Método | Endpoint | Descripción |
|--------|----------|-------------|
| GET | `/api/doctores` | Listar doctores |
| POST | `/api/doctores` | Crear doctor |
| PUT | `/api/doctores/{id}` | Actualizar doctor |
| DELETE | `/api/doctores/{id}` | Eliminar doctor |

### 📅 Citas
| Método | Endpoint | Descripción |
|--------|----------|-------------|
| GET | `/api/citas` | Listar citas |
| POST | `/api/citas` | Crear cita |
| PUT | `/api/citas/{id}` | Actualizar cita |
| DELETE | `/api/citas/{id}` | Cancelar cita |
| GET | `/api/citas/disponibilidad` | Horarios disponibles |

### 📋 Historia Clínica
| Método | Endpoint | Descripción |
|--------|----------|-------------|
| GET | `/api/historias/paciente/{id}` | Historial de paciente |
| POST | `/api/historias` | Crear historia |
| PUT | `/api/historias/{id}` | Actualizar historia |

### 💊 Recetas
| Método | Endpoint | Descripción |
|--------|----------|-------------|
| GET | `/api/recetas/paciente/{id}` | Recetas del paciente |
| POST | `/api/recetas` | Crear receta |
| GET | `/api/recetas/{id}/pdf` | Descargar PDF |

### 💳 Pagos
| Método | Endpoint | Descripción |
|--------|----------|-------------|
| GET | `/api/pagos` | Listar pagos |
| POST | `/api/pagos` | Registrar pago |

---

## 📊 Base de Datos

### Entidades Principales

- **Usuario** - Email, PasswordHash, Nombre, Apellido, Rol
- **Doctor** - Especialidad, Licencia, Horario, Precio
- **Cita** - PacienteId, DoctorId, FechaHora, Estado
- **HistoriaClinica** - PacienteId, DoctorId, Diagnóstico
- **Receta** - HistoriaClinicaId, Medicamentos, Instrucciones
- **Pago** - CitaId, Monto, Estado

---

## 🎨 Interfaz de Usuario

### Dashboard - Roles

- **Paciente**: Mis Citas, Reservar Cita, Historial, Perfil
- **Doctor**: Citas del Día, Mis Pacientes, Historial Clínico
- **Admin**: Doctores, Todas las Citas, Pagos, Usuarios

---

## 📝 Notas de Desarrollo

- El proyecto usa **SQLite** por defecto para desarrollo local sin Docker
- Para producción, cambiar a **PostgreSQL** en `appsettings.json`
- Las recetas se generan como texto plano (puede mejorarse con QuestPDF)
- El frontend usa HttpClient directo para simplificar la autenticación

---

## 📄 Licencia

Este proyecto es de uso educativo y demostrativo.

---

<p align="center">
  ❤️ Desarrollado con .NET 10 y Blazor
</p>