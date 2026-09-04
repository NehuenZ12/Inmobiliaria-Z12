--
-- PostgreSQL - RESET COMPLETO de la base "inmobiliaria"
-- Borra TODO lo que exista (sin importar el orden ni el estado actual)
-- y recrea la estructura completa desde cero, con datos de ejemplo.
--
-- Roles de Usuario: 'Administrador' y 'Empleado'
--

SET statement_timeout = 0;
SET lock_timeout = 0;
SET idle_in_transaction_session_timeout = 0;
SET client_encoding = 'UTF8';
SET standard_conforming_strings = on;
SET check_function_bodies = false;
SET xmloption = content;
SET client_min_messages = warning;
SET row_security = off;
SET default_tablespace = '';
SET default_table_access_method = heap;

--
-- 0) Borrado total, en cualquier orden, CASCADE se encarga de las dependencias
--

DROP TABLE IF EXISTS public.pago CASCADE;
DROP TABLE IF EXISTS public.reserva CASCADE;
DROP TABLE IF EXISTS public.imagen CASCADE;
DROP TABLE IF EXISTS public.inquilino CASCADE;
DROP TABLE IF EXISTS public.usuario CASCADE;
DROP TABLE IF EXISTS public.inmueble CASCADE;
DROP TABLE IF EXISTS public.tipo CASCADE;
DROP TABLE IF EXISTS public.propietario CASCADE;

--
-- 1) propietario
--

CREATE TABLE public.propietario (
    id integer NOT NULL,
    nombre character varying(100) NOT NULL,
    apellido character varying(100) NOT NULL,
    dni character varying(20) NOT NULL,
    telefono character varying(30),
    email character varying(150)
);

CREATE SEQUENCE public.propietario_id_seq
    AS integer START WITH 1 INCREMENT BY 1 NO MINVALUE NO MAXVALUE CACHE 1;

ALTER SEQUENCE public.propietario_id_seq OWNED BY public.propietario.id;
ALTER TABLE ONLY public.propietario ALTER COLUMN id SET DEFAULT nextval('public.propietario_id_seq'::regclass);
ALTER TABLE ONLY public.propietario ADD CONSTRAINT propietario_pkey PRIMARY KEY (id);

--
-- 2) tipo
--

CREATE TABLE public.tipo (
    id integer NOT NULL,
    nombre character varying(50) NOT NULL,
    descripcion character varying(255)
);

CREATE SEQUENCE public.tipo_id_seq
    AS integer START WITH 1 INCREMENT BY 1 NO MINVALUE NO MAXVALUE CACHE 1;

ALTER SEQUENCE public.tipo_id_seq OWNED BY public.tipo.id;
ALTER TABLE ONLY public.tipo ALTER COLUMN id SET DEFAULT nextval('public.tipo_id_seq'::regclass);
ALTER TABLE ONLY public.tipo ADD CONSTRAINT tipo_pkey PRIMARY KEY (id);
ALTER TABLE ONLY public.tipo ADD CONSTRAINT tipo_nombre_key UNIQUE (nombre);

--
-- 3) inmueble (depende de propietario y tipo)
--

CREATE TABLE public.inmueble (
    id integer NOT NULL,
    direccion character varying(200) NOT NULL,
    cupo integer NOT NULL,
    latitud numeric(10,7),
    longitud numeric(10,7),
    precio_por_dia numeric(10,2) NOT NULL,
    porcentaje_reserva numeric(5,2) NOT NULL,
    disponible boolean DEFAULT true NOT NULL,
    descripcion character varying(500),
    propietario_id integer NOT NULL,
    tipo_id integer NOT NULL
);

CREATE SEQUENCE public.inmueble_id_seq
    AS integer START WITH 1 INCREMENT BY 1 NO MINVALUE NO MAXVALUE CACHE 1;

ALTER SEQUENCE public.inmueble_id_seq OWNED BY public.inmueble.id;
ALTER TABLE ONLY public.inmueble ALTER COLUMN id SET DEFAULT nextval('public.inmueble_id_seq'::regclass);
ALTER TABLE ONLY public.inmueble ADD CONSTRAINT inmueble_pkey PRIMARY KEY (id);
ALTER TABLE ONLY public.inmueble
    ADD CONSTRAINT inmueble_propietario_id_fkey FOREIGN KEY (propietario_id) REFERENCES public.propietario(id);
ALTER TABLE ONLY public.inmueble
    ADD CONSTRAINT inmueble_tipo_id_fkey FOREIGN KEY (tipo_id) REFERENCES public.tipo(id);

--
-- 4) imagen (depende de inmueble)
--

CREATE TABLE public.imagen (
    id integer NOT NULL,
    url character varying(500) NOT NULL,
    descripcion character varying(255),
    es_principal boolean DEFAULT false NOT NULL,
    inmueble_id integer NOT NULL
);

CREATE SEQUENCE public.imagen_id_seq
    AS integer START WITH 1 INCREMENT BY 1 NO MINVALUE NO MAXVALUE CACHE 1;

ALTER SEQUENCE public.imagen_id_seq OWNED BY public.imagen.id;
ALTER TABLE ONLY public.imagen ALTER COLUMN id SET DEFAULT nextval('public.imagen_id_seq'::regclass);
ALTER TABLE ONLY public.imagen ADD CONSTRAINT imagen_pkey PRIMARY KEY (id);
ALTER TABLE ONLY public.imagen
    ADD CONSTRAINT imagen_inmueble_id_fkey FOREIGN KEY (inmueble_id) REFERENCES public.inmueble(id);

--
-- 5) usuario
--

CREATE TABLE public.usuario (
    id_usuario integer NOT NULL,
    nombre character varying(100) NOT NULL,
    apellido character varying(100) NOT NULL,
    email character varying(150) NOT NULL,
    clave character varying(255) NOT NULL,
    avatar character varying(255),
    rol character varying(20) NOT NULL,
    activo boolean DEFAULT true NOT NULL,
    fecha_alta timestamp DEFAULT now() NOT NULL,
    CONSTRAINT usuario_rol_check CHECK (((rol)::text = ANY ((ARRAY['Administrador'::character varying, 'Empleado'::character varying])::text[])))
);

CREATE SEQUENCE public.usuario_id_usuario_seq
    AS integer START WITH 1 INCREMENT BY 1 NO MINVALUE NO MAXVALUE CACHE 1;

ALTER SEQUENCE public.usuario_id_usuario_seq OWNED BY public.usuario.id_usuario;
ALTER TABLE ONLY public.usuario ALTER COLUMN id_usuario SET DEFAULT nextval('public.usuario_id_usuario_seq'::regclass);
ALTER TABLE ONLY public.usuario ADD CONSTRAINT usuario_pkey PRIMARY KEY (id_usuario);
ALTER TABLE ONLY public.usuario ADD CONSTRAINT usuario_email_key UNIQUE (email);

--
-- 6) inquilino
--

CREATE TABLE public.inquilino (
    id integer NOT NULL,
    nombre character varying(100) NOT NULL,
    apellido character varying(100) NOT NULL,
    dni character varying(20) NOT NULL,
    telefono character varying(30),
    email character varying(150),
    fecha_alta timestamp DEFAULT now() NOT NULL
);

CREATE SEQUENCE public.inquilino_id_seq
    AS integer START WITH 1 INCREMENT BY 1 NO MINVALUE NO MAXVALUE CACHE 1;

ALTER SEQUENCE public.inquilino_id_seq OWNED BY public.inquilino.id;
ALTER TABLE ONLY public.inquilino ALTER COLUMN id SET DEFAULT nextval('public.inquilino_id_seq'::regclass);
ALTER TABLE ONLY public.inquilino ADD CONSTRAINT inquilino_pkey PRIMARY KEY (id);

--
-- 7) reserva (depende de usuario, inquilino, inmueble)
--

CREATE TABLE public.reserva (
    id integer NOT NULL,
    usuario_creador_id integer NOT NULL,
    usuario_terminador_id integer,
    inquilino_id integer NOT NULL,
    inmueble_id integer NOT NULL,
    fecha_desde date NOT NULL,
    fecha_hasta date NOT NULL,
    monto_diario numeric(10,2) NOT NULL,
    cantidad_personas integer NOT NULL,
    estado character varying(20) DEFAULT 'Pendiente'::character varying NOT NULL,
    fecha_creacion timestamp DEFAULT now() NOT NULL,
    fecha_terminacion date,
    CONSTRAINT reserva_estado_check CHECK (((estado)::text = ANY ((ARRAY['Pendiente'::character varying, 'Confirmada'::character varying, 'Cancelada'::character varying, 'Completada'::character varying, 'Expirada'::character varying])::text[]))),
    CONSTRAINT reserva_fechas_check CHECK (fecha_hasta > fecha_desde)
);

CREATE SEQUENCE public.reserva_id_seq
    AS integer START WITH 1 INCREMENT BY 1 NO MINVALUE NO MAXVALUE CACHE 1;

ALTER SEQUENCE public.reserva_id_seq OWNED BY public.reserva.id;
ALTER TABLE ONLY public.reserva ALTER COLUMN id SET DEFAULT nextval('public.reserva_id_seq'::regclass);
ALTER TABLE ONLY public.reserva ADD CONSTRAINT reserva_pkey PRIMARY KEY (id);
ALTER TABLE ONLY public.reserva
    ADD CONSTRAINT reserva_inquilino_id_fkey FOREIGN KEY (inquilino_id) REFERENCES public.inquilino(id);
ALTER TABLE ONLY public.reserva
    ADD CONSTRAINT reserva_inmueble_id_fkey FOREIGN KEY (inmueble_id) REFERENCES public.inmueble(id);
ALTER TABLE ONLY public.reserva
    ADD CONSTRAINT reserva_usuario_creador_id_fkey FOREIGN KEY (usuario_creador_id) REFERENCES public.usuario(id_usuario);
ALTER TABLE ONLY public.reserva
    ADD CONSTRAINT reserva_usuario_terminador_id_fkey FOREIGN KEY (usuario_terminador_id) REFERENCES public.usuario(id_usuario);

--
-- 8) pago (depende de reserva, usuario)
--

CREATE TABLE public.pago (
    id integer NOT NULL,
    fecha date NOT NULL,
    concepto character varying(200) NOT NULL,
    importe numeric(12,2) NOT NULL,
    reserva_id integer NOT NULL,
    anulado boolean DEFAULT false NOT NULL,
    usuario_creador_id integer NOT NULL,
    usuario_anulador_id integer,
    metodo character varying(20) NOT NULL,
    estado character varying(20) DEFAULT 'Pendiente'::character varying NOT NULL,
    comprobante_url character varying(500),
    CONSTRAINT pago_metodo_check CHECK (((metodo)::text = ANY ((ARRAY['Efectivo'::character varying, 'Transferencia'::character varying, 'Tarjeta'::character varying, 'MercadoPago'::character varying])::text[]))),
    CONSTRAINT pago_estado_check CHECK (((estado)::text = ANY ((ARRAY['Pendiente'::character varying, 'Pagado'::character varying, 'Anulado'::character varying, 'Rechazado'::character varying])::text[])))
);

CREATE SEQUENCE public.pago_id_seq
    AS integer START WITH 1 INCREMENT BY 1 NO MINVALUE NO MAXVALUE CACHE 1;

ALTER SEQUENCE public.pago_id_seq OWNED BY public.pago.id;
ALTER TABLE ONLY public.pago ALTER COLUMN id SET DEFAULT nextval('public.pago_id_seq'::regclass);
ALTER TABLE ONLY public.pago ADD CONSTRAINT pago_pkey PRIMARY KEY (id);
ALTER TABLE ONLY public.pago
    ADD CONSTRAINT pago_reserva_id_fkey FOREIGN KEY (reserva_id) REFERENCES public.reserva(id);
ALTER TABLE ONLY public.pago
    ADD CONSTRAINT pago_usuario_creador_id_fkey FOREIGN KEY (usuario_creador_id) REFERENCES public.usuario(id_usuario);
ALTER TABLE ONLY public.pago
    ADD CONSTRAINT pago_usuario_anulador_id_fkey FOREIGN KEY (usuario_anulador_id) REFERENCES public.usuario(id_usuario);

--
-- 9) Datos de ejemplo
--

-- INSERT INTO public.tipo (nombre) VALUES
--    ('Casa'), ('Departamento'), ('Monoambiente'),
--    ('Cabaña'), ('Local'), ('Oficina'), ('Terreno');

-- INSERT INTO public.propietario (nombre, apellido, dni, telefono, email) VALUES
--    ('Nehuen', 'Zerdá', '42999123', '2657799870', 'nehuen123@gmail.com'),
--    ('Heber', 'Gomez', '40123321', '2664995566', 'hebergomez@gmail.com'),
--    ('Jose', 'Garces', '39098123', '2664774455', 'josegarces@gmail.com');

-- INSERT INTO public.inmueble (direccion, cupo, latitud, longitud, precio_por_dia, porcentaje_reserva, disponible, propietario_id, tipo_id) VALUES
--    ('25 de mayo 950 San Luis', 2, 20.0000000, 15.0000000, 50.00, 50.00, true,
--        (SELECT id FROM public.propietario WHERE dni = '42999123'),
--        (SELECT id FROM public.tipo WHERE nombre = 'Casa')),
--    ('Sarmiento 532 San Luis', 2, 10.0000000, 5.0000000, 25.00, 30.00, true,
--        (SELECT id FROM public.propietario WHERE dni = '39098123'),
--        (SELECT id FROM public.tipo WHERE nombre = 'Monoambiente')),
--    ('Av centenario 245 San Luis', 3, 30.0000000, 20.0000000, 45.00, 50.00, true,
--        (SELECT id FROM public.propietario WHERE dni = '40123321'),
--        (SELECT id FROM public.tipo WHERE nombre = 'Casa'));

-- Usuario administrador de ejemplo (clave en texto plano solo para pruebas
-- de la base -- tu aplicacion deberia hashear la clave antes de insertarla)
-- INSERT INTO public.usuario (nombre, apellido, email, clave, rol) VALUES
--    ('Admin', 'Sistema', 'admin@inmobiliaria.com', 'cambiar_esta_clave', 'Administrador');

--
-- Verificacion final: deberia mostrar las 8 tablas creadas
--

SELECT table_name
FROM information_schema.tables
WHERE table_schema = 'public'
ORDER BY table_name;