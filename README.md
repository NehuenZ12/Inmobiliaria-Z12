# Proyecto Inmobiliaria

Proyecto Inmobiliaria Laboratorio 2

---

## 👥 Integrantes del Grupo

- **Aldo Nehuen Zerdá** - *aldonehuen123@gmail.com* - [@NehuenZ12](https://github.com/nehuenZ12)
- **Heber Gomez** - *heber12398@gmail.com* - [@owengmz](https://github.com/owengmz) - Discord: `usuario_discord`
- **José Gabriel Garces Brocal** - *jjoosseegg69@gmail.com* - [@josegarcesss](https://github.com/josegarcesss) - Discord: `usuario_discord`

---

## 📐 Modelado de Datos

A continuación se presenta el esquema del modelo de datos correspondiente a la aplicación:

### Diagrama Entidad-Relación (DER) / Diagrama de Clases

![Diagrama del proyecto Entidad-Relación (DER) ](./Diagrama2.png)


El proyecto utiliza PostgreSQL.

Requisitos

- PostgreSQL instalado y ejecutándose.

- DBeaver (opcional, recomendado para administrar la base de datos).

- .NET SDK compatible con el proyecto.


Crear y levantar la base de datos

1_ Abrir PostgreSQL/DBeaver y conectarse al servidor localhost:5432.

2_ Crear una base de datos llamada:

inmobiliaria

En DBeaver se puede hacer con:

CREATE DATABASE inmobiliaria;

3_ Abrir un SQL Editor conectado a la base inmobiliaria.

4_ Abrir el archivo inmobiliaria.sql que se encuentra en el proyecto.

5_ Ejecutar todo el script.

6_ El script crea las 8 tablas del modelo (propietario, tipo, inmueble, imagen, usuario, inquilino, reserva y pago), crea las claves foraneas entre ellas y carga datos iniciales de prueba.

7_ Verificar que las tablas aparezcan dentro de:

inmobiliaria > Schemas > public > Tables

Conexión del proyecto

Configurar la cadena de conexión en appsettings.json con los datos del PostgreSQL local:

"ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=inmobiliaria;Username=postgres;Password=TU_CONTRASEÑA"
}

Ejecutar el proyecto:

Desde la carpeta que contiene mvc.csproj:

dotnet restore
dotnet run

Luego abrir la URL que indique la terminal.