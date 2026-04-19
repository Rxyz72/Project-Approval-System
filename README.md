## Project Overview
The Project Approval System (PAS) is a secure, web-based platform designed for academic institutions to facilitate the matching of student research projects with faculty supervisors.
The core innovation of this system is the "Blind-Matching" mechanism. To eliminate unconscious bias and ensure selection is based purely on technical merit, supervisors browse project proposals anonymously. The identities of both the student and the supervisor are only revealed once a formal match is confirmed.

## Tech Stack
Framework: ASP.NET Core
Database: SQL Server
ORM: Entity Framework (EF) Core (with Migrations)
Security: Role-Based Access Control (RBAC)
Testing: xUnit / Moq (Unit & Integration Testing)

 ## Key Features
1. Blind-Match Logic
Anonymity: Supervisors view Title, Abstract, Tech Stack, and Research Area without student metadata.
Identity Reveal: A two-way disclosure of contact details triggered only upon a confirmed match.

2. Role-Based Functionalities
Students: Submit proposals, track status (Pending/Matched), and view supervisor details post-match.
Supervisors: Manage expertise tags, browse anonymous dashboard, and express interest in projects.
Module Leader: Oversight dashboard, manual reassignment, and research area management.
Admin: User account management and environment configuration.
