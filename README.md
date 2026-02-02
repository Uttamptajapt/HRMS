HRMS – Human Resource Management System

A role-based Human Resource Management System (HRMS) built using ASP.NET Core following Clean Architecture principles.
This project is designed to manage employees, HR operations, and organizational data securely and efficiently.

📌 Project Overview

The HRMS project provides a centralized platform for managing employee data, HR users, and administrative operations within an organization.
It supports multiple user roles, secure authentication using JWT, and scalable backend architecture suitable for enterprise-level applications.

🏗️ Architecture

This project follows Clean Architecture for better maintainability and scalability.

HRMS
│
├── HRMS.API              → API Layer (Controllers)
├── HRMS.Application      → Business Logic, DTOs, Interfaces
├── HRMS.Infrastructure   → Database, Identity, Repositories
└── HRMS.Domain           → Core Entities & Models

🚀 Technology Stack

Backend Framework: ASP.NET Core (.NET 8)

Language: C#

Database: PostgreSQL

ORM: Entity Framework Core

Authentication: JWT (JSON Web Token)

Authorization: Role-based (Admin, HR, Employee)

API Style: RESTful APIs

Version Control: Git & GitHub

🔐 Authentication & Authorization

Secure login using JWT tokens

Token-based authorization for APIs

Role-based access control implemented using ASP.NET Identity

👥 User Roles & Permissions
🔑 Admin

Create and manage HR users

View all employees across the organization

Manage organizational data

🧑‍💼 HR

Manage (CRUD) only those employees created by themselves

Cannot manage Admin users

Limited access based on organization scope

👨‍💻 Employee

View only their own profile

No create/update/delete permissions

🗄️ Database Design

PostgreSQL database

Entity Framework Core migrations

Relationships handled using foreign keys

Scalable for multiple organizations

📦 Key Features

✅ User registration & login

✅ JWT-based authentication

✅ Role-based authorization

✅ Employee management

✅ HR management by Admin

✅ Clean Architecture implementation

✅ Secure REST APIs

✅ Scalable database structure

🔄 CI/CD & DevOps (Optional / In Progress)

GitHub repository with proper branch strategy

CI/CD pipeline using Azure DevOps (planned / implemented)

Automated build and deployment support
