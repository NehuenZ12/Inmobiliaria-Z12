--
-- PostgreSQL - Script de actualizacion completa
-- Alinea la base "inmobiliaria" con el diagrama de clases y la narrativa del proyecto.
--
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
-- Limpieza previa de tablas que vamos a (re)crear desde cero
-- (no toca "propietario" ni "inmueble")
--

DROP TABLE IF EXISTS public.pago CASCADE;
DROP TABLE IF EXISTS public.reserva CASCADE;
DROP TABLE IF EXISTS public.inquilino CASCADE;
DROP TABLE IF EXISTS public.usuario CASCADE;
DROP TABLE IF EXISTS public.imagen CASCADE;
DROP TABLE IF EXISTS public.tipo CASCADE;

--
-- 1) Name: tipo; Type: TABLE
--

CREATE TABLE public.tipo (
    id integer NOT NULL,
    nombre character varying(50) NOT NULL,
    descripcion character varying(255)
);

CREATE SEQUENCE public.tipo_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;

ALTER SEQUENCE public.tipo_id_seq OWNED BY public.tipo.id;
ALTER TABLE ONLY public.tipo ALTER COLUMN id SET DEFAULT nextval('public.tipo_id_seq'::regclass);
ALTER TABLE ONLY public.tipo ADD CONSTRAINT tipo_pkey PRIMARY KEY (id);
ALTER TABLE ONLY public.tipo ADD CONSTRAINT tipo_nombre_key UNIQUE (nombre);

-- Migramos los valores que ya existian como texto en inmueble.tipo,
-- normalizando mayusculas/minusculas (ej: "casa" y "Casa" quedan como "Casa")
INSERT INTO public.tipo (nombre)
SELECT DISTINCT INITCAP(TRIM(tipo))
FROM public.inmueble
ON CONFLICT (nombre) DO NOTHING;

SELECT pg_catalog.setval('public.tipo_id_seq', GREATEST((SELECT MAX(id) FROM public.tipo), 1), true);

--
-- 2) Migracion de "inmueble": agregar tipo_id (FK) y descripcion,
--    sin perder los datos existentes
--

ALTER TABLE public.inmueble ADD COLUMN IF NOT EXISTS tipo_id integer;
ALTER TABLE public.inmueble ADD COLUMN IF NOT EXISTS descripcion character varying(500);

UPDATE public.inmueble i
SET tipo_id = t.id
FROM public.tipo t
WHERE t.nombre = INITCAP(TRIM(i.tipo));

ALTER TABLE public.inmueble ALTER COLUMN tipo_id SET NOT NULL;
ALTER TABLE public.inmueble
    ADD CONSTRAINT inmueble_tipo_id_fkey FOREIGN KEY (tipo_id) REFERENCES public.tipo(id);

-- La columna vieja de texto "tipo" ya quedo reemplazada por tipo_id.
-- Si preferis conservarla por las dudas, comenta la siguiente linea.
ALTER TABLE public.inmueble DROP COLUMN tipo;

--
-- 3) Name: imagen; Type: TABLE (depende de inmueble)
--

CREATE TABLE public.imagen (
    id integer NOT NULL,
    url character varying(500) NOT NULL,
    descripcion character varying(255),
    es_principal boolean DEFAULT false NOT NULL,
    inmueble_id integer NOT NULL
);

CREATE SEQUENCE public.imagen_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;

ALTER SEQUENCE public.imagen_id_seq OWNED BY public.imagen.id;
ALTER TABLE ONLY public.imagen ALTER COLUMN id SET DEFAULT nextval('public.imagen_id_seq'::regclass);
ALTER TABLE ONLY public.imagen ADD CONSTRAINT imagen_pkey PRIMARY KEY (id);
ALTER TABLE ONLY public.imagen
    ADD CONSTRAINT imagen_inmueble_id_fkey FOREIGN KEY (inmueble_id) REFERENCES public.inmueble(id);

--
-- 4) Name: usuario; Type: TABLE
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
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;

ALTER SEQUENCE public.usuario_id_usuario_seq OWNED BY public.usuario.id_usuario;
ALTER TABLE ONLY public.usuario ALTER COLUMN id_usuario SET DEFAULT nextval('public.usuario_id_usuario_seq'::regclass);
ALTER TABLE ONLY public.usuario ADD CONSTRAINT usuario_pkey PRIMARY KEY (id_usuario);
ALTER TABLE ONLY public.usuario ADD CONSTRAINT usuario_email_key UNIQUE (email);

--
-- 5) Name: inquilino; Type: TABLE
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
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;

ALTER SEQUENCE public.inquilino_id_seq OWNED BY public.inquilino.id;
ALTER TABLE ONLY public.inquilino ALTER COLUMN id SET DEFAULT nextval('public.inquilino_id_seq'::regclass);
ALTER TABLE ONLY public.inquilino ADD CONSTRAINT inquilino_pkey PRIMARY KEY (id);

--
-- 6) Name: reserva; Type: TABLE (depende de usuario, inquilino, inmueble)
--    Version completa: incluye todos los campos de negocio que pide
--    la narrativa y el diagrama (antes solo tenia el "esqueleto" de auditoria)
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
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;

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
-- 7) Name: pago; Type: TABLE (depende de reserva, usuario)
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
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;

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
-- Fin del script
--
