--
-- PostgreSQL database dump (CORREGIDO)
--
-- Correcciones aplicadas:
--   1. La tabla "inquilino" se crea ANTES de "reserva", porque "reserva"
--      tiene una FK hacia "inquilino". En el archivo original, "inquilino"
--      se creaba al final, lo que hacia fallar la ejecucion.
--   2. Se elimino un fragmento de texto invalido ("-git") que habia quedado
--      pegado dentro de un comentario SQL, mezclado con una sentencia ALTER TABLE.
--   3. Se agregaron DROP TABLE IF EXISTS ... CASCADE al inicio para que el
--      script se pueda ejecutar sin error aunque las tablas ya existan.
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
-- Limpieza previa (evita error "relation already exists" si corres el script mas de una vez)
--

DROP TABLE IF EXISTS public.pago CASCADE;
DROP TABLE IF EXISTS public.reserva CASCADE;
DROP TABLE IF EXISTS public.inquilino CASCADE;
DROP TABLE IF EXISTS public.usuario CASCADE;
DROP TABLE IF EXISTS public.inmueble CASCADE;
DROP TABLE IF EXISTS public.propietario CASCADE;

--
-- Name: propietario; Type: TABLE
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
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;

ALTER SEQUENCE public.propietario_id_seq OWNED BY public.propietario.id;
ALTER TABLE ONLY public.propietario ALTER COLUMN id SET DEFAULT nextval('public.propietario_id_seq'::regclass);
ALTER TABLE ONLY public.propietario ADD CONSTRAINT propietario_pkey PRIMARY KEY (id);

--
-- Name: inmueble; Type: TABLE (depende de propietario)
--

CREATE TABLE public.inmueble (
    id integer NOT NULL,
    direccion character varying(200) NOT NULL,
    cupo integer NOT NULL,
    tipo character varying(50) NOT NULL,
    latitud numeric(10,7),
    longitud numeric(10,7),
    precio_por_dia numeric(10,2) NOT NULL,
    porcentaje_reserva numeric(5,2) NOT NULL,
    disponible boolean DEFAULT true NOT NULL,
    propietario_id integer NOT NULL
);

CREATE SEQUENCE public.inmueble_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;

ALTER SEQUENCE public.inmueble_id_seq OWNED BY public.inmueble.id;
ALTER TABLE ONLY public.inmueble ALTER COLUMN id SET DEFAULT nextval('public.inmueble_id_seq'::regclass);
ALTER TABLE ONLY public.inmueble ADD CONSTRAINT inmueble_pkey PRIMARY KEY (id);
ALTER TABLE ONLY public.inmueble
    ADD CONSTRAINT inmueble_propietario_id_fkey FOREIGN KEY (propietario_id) REFERENCES public.propietario(id);

--
-- Name: usuario; Type: TABLE
--

CREATE TABLE public.usuario (
    id_usuario integer NOT NULL,
    nombre character varying(100) NOT NULL,
    apellido character varying(100) NOT NULL,
    email character varying(150) NOT NULL,
    clave character varying(255) NOT NULL,
    avatar character varying(255),
    rol character varying(20) NOT NULL,
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
-- Name: inquilino; Type: TABLE
-- (Movida antes de "reserva" porque reserva la referencia por FK)
--

CREATE TABLE public.inquilino (
    id integer NOT NULL,
    nombre character varying(100) NOT NULL,
    apellido character varying(100) NOT NULL,
    dni character varying(20) NOT NULL,
    telefono character varying(30),
    email character varying(150)
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
-- Name: reserva; Type: TABLE (depende de usuario, inquilino, inmueble)
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
    fecha_terminacion date
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
-- Name: pago; Type: TABLE (depende de reserva, usuario)
--

CREATE TABLE public.pago (
    id integer NOT NULL,
    fecha date NOT NULL,
    concepto character varying(200) NOT NULL,
    importe numeric(12,2) NOT NULL,
    reserva_id integer NOT NULL,
    anulado boolean DEFAULT false NOT NULL,
    usuario_creador_id integer NOT NULL,
    usuario_anulador_id integer
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
-- Data for Name: propietario
--

INSERT INTO public.propietario VALUES (1, 'Nehuen ', 'Zerdá', '42999123', '2657799870', 'nehuen123@gmail.com');
INSERT INTO public.propietario VALUES (2, 'Heber', 'Gomez', '40123321', '2664995566', 'hebergomez@gmail.com');
INSERT INTO public.propietario VALUES (3, 'Jose', 'Garces', '39098123', '2664774455', 'josegarces@gmail.com');

--
-- Data for Name: inmueble
--

INSERT INTO public.inmueble VALUES (1, '25 de mayo 950 San Luis', 2, 'casa', 20.0000000, 15.0000000, 50.00, 50.00, true, 1);
INSERT INTO public.inmueble VALUES (3, 'Sarmiento 532 San Luis', 2, 'Monoambiente', 10.0000000, 5.0000000, 25.00, 30.00, true, 3);
INSERT INTO public.inmueble VALUES (4, 'Av centenario 245 San Luis', 3, 'Casa', 30.0000000, 20.0000000, 45.00, 50.00, true, 2);

--
-- Sequence values
--

SELECT pg_catalog.setval('public.propietario_id_seq', 3, true);
SELECT pg_catalog.setval('public.inmueble_id_seq', 4, true);
SELECT pg_catalog.setval('public.usuario_id_usuario_seq', 1, false);
SELECT pg_catalog.setval('public.inquilino_id_seq', 1, false);
SELECT pg_catalog.setval('public.reserva_id_seq', 1, false);
SELECT pg_catalog.setval('public.pago_id_seq', 1, false);

--
-- PostgreSQL database dump complete
--