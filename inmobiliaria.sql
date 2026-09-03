--
-- PostgreSQL database dump
--

\restrict QtSBGeRRY6j6Jwo1fkh8gtkMmOFbsIZaIOddumQrOTNweMS0szVWYyfIOxsVWS0

-- Dumped from database version 18.4
-- Dumped by pg_dump version 18.4

-- Started on 2026-08-20 19:49:08

SET statement_timeout = 0;
SET lock_timeout = 0;
SET idle_in_transaction_session_timeout = 0;
SET transaction_timeout = 0;
SET client_encoding = 'UTF8';
SET standard_conforming_strings = on;
SELECT pg_catalog.set_config('search_path', '', false);
SET check_function_bodies = false;
SET xmloption = content;
SET client_min_messages = warning;
SET row_security = off;

SET default_tablespace = '';

SET default_table_access_method = heap;

--
-- TOC entry 222 (class 1259 OID 16725)
-- Name: inmueble; Type: TABLE; Schema: public; Owner: -
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


--
-- TOC entry 221 (class 1259 OID 16724)
-- Name: inmueble_id_seq; Type: SEQUENCE; Schema: public; Owner: -
--

CREATE SEQUENCE public.inmueble_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


--
-- TOC entry 5025 (class 0 OID 0)
-- Dependencies: 221
-- Name: inmueble_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: -
--

ALTER SEQUENCE public.inmueble_id_seq OWNED BY public.inmueble.id;


--
-- TOC entry 220 (class 1259 OID 16714)
-- Name: propietario; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public.propietario (
    id integer NOT NULL,
    nombre character varying(100) NOT NULL,
    apellido character varying(100) NOT NULL,
    dni character varying(20) NOT NULL,
    telefono character varying(30),
    email character varying(150)
);


--
-- TOC entry 219 (class 1259 OID 16713)
-- Name: propietario_id_seq; Type: SEQUENCE; Schema: public; Owner: -
--

CREATE SEQUENCE public.propietario_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


--
-- TOC entry 5026 (class 0 OID 0)
-- Dependencies: 219
-- Name: propietario_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: -
--

ALTER SEQUENCE public.propietario_id_seq OWNED BY public.propietario.id;


--
-- Name: usuario; Type: TABLE; Schema: public; Owner: -
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


--
-- Name: usuario_id_usuario_seq; Type: SEQUENCE; Schema: public; Owner: -
--

CREATE SEQUENCE public.usuario_id_usuario_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


--
-- Name: usuario_id_usuario_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: -
--

ALTER SEQUENCE public.usuario_id_usuario_seq OWNED BY public.usuario.id_usuario;


--
-- Name: reserva; Type: TABLE; Schema: public; Owner: -
--
-- Estructura base con campos de auditoría.
-- Las columnas de negocio adicionales las puede incorporar el módulo de reservas.
--

CREATE TABLE public.reserva (
    id integer NOT NULL,
    usuario_creador_id integer NOT NULL,
    usuario_terminador_id integer
);


--
-- Name: reserva_id_seq; Type: SEQUENCE; Schema: public; Owner: -
--

CREATE SEQUENCE public.reserva_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


--
-- Name: reserva_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: -
--

ALTER SEQUENCE public.reserva_id_seq OWNED BY public.reserva.id;


--
-- Name: pago; Type: TABLE; Schema: public; Owner: -
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
    comprobante_url character varying(500)
);


--
-- Name: pago_id_seq; Type: SEQUENCE; Schema: public; Owner: -
--

CREATE SEQUENCE public.pago_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


--
-- Name: pago_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: -
--

ALTER SEQUENCE public.pago_id_seq OWNED BY public.pago.id;


--
-- TOC entry 4862 (class 2604 OID 16728)
-- Name: inmueble id; Type: DEFAULT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.inmueble ALTER COLUMN id SET DEFAULT nextval('public.inmueble_id_seq'::regclass);


--
-- TOC entry 4861 (class 2604 OID 16717)
-- Name: propietario id; Type: DEFAULT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.propietario ALTER COLUMN id SET DEFAULT nextval('public.propietario_id_seq'::regclass);


--
-- Name: usuario id_usuario; Type: DEFAULT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.usuario ALTER COLUMN id_usuario SET DEFAULT nextval('public.usuario_id_usuario_seq'::regclass);


--
-- Name: reserva id; Type: DEFAULT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.reserva ALTER COLUMN id SET DEFAULT nextval('public.reserva_id_seq'::regclass);


--
-- Name: pago id; Type: DEFAULT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.pago ALTER COLUMN id SET DEFAULT nextval('public.pago_id_seq'::regclass);


--
-- TOC entry 5019 (class 0 OID 16725)
-- Dependencies: 222
-- Data for Name: inmueble; Type: TABLE DATA; Schema: public; Owner: -
--

INSERT INTO public.inmueble VALUES (1, '25 de mayo 950 San Luis', 2, 'casa', 20.0000000, 15.0000000, 50.00, 50.00, true, 1);
INSERT INTO public.inmueble VALUES (3, 'Sarmiento 532 San Luis', 2, 'Monoambiente', 10.0000000, 5.0000000, 25.00, 30.00, true, 3);
INSERT INTO public.inmueble VALUES (4, 'Av centenario 245 San Luis', 3, 'Casa', 30.0000000, 20.0000000, 45.00, 50.00, true, 2);


--
-- TOC entry 5017 (class 0 OID 16714)
-- Dependencies: 220
-- Data for Name: propietario; Type: TABLE DATA; Schema: public; Owner: -
--

INSERT INTO public.propietario VALUES (1, 'Nehuen ', 'Zerdá', '42999123', '2657799870', 'nehuen123@gmail.com');
INSERT INTO public.propietario VALUES (2, 'Heber', 'Gomez', '40123321', '2664995566', 'hebergomez@gmail.com');
INSERT INTO public.propietario VALUES (3, 'Jose', 'Garces', '39098123', '2664774455', 'josegarces@gmail.com');


--
-- TOC entry 5027 (class 0 OID 0)
-- Dependencies: 221
-- Name: inmueble_id_seq; Type: SEQUENCE SET; Schema: public; Owner: -
--

SELECT pg_catalog.setval('public.inmueble_id_seq', 4, true);


--
-- TOC entry 5028 (class 0 OID 0)
-- Dependencies: 219
-- Name: propietario_id_seq; Type: SEQUENCE SET; Schema: public; Owner: -
--

SELECT pg_catalog.setval('public.propietario_id_seq', 4, true);


--
-- Name: usuario_id_usuario_seq; Type: SEQUENCE SET; Schema: public; Owner: -
--

SELECT pg_catalog.setval('public.usuario_id_usuario_seq', 1, false);


--
-- Name: reserva_id_seq; Type: SEQUENCE SET; Schema: public; Owner: -
--

SELECT pg_catalog.setval('public.reserva_id_seq', 1, false);


--
-- Name: pago_id_seq; Type: SEQUENCE SET; Schema: public; Owner: -
--

SELECT pg_catalog.setval('public.pago_id_seq', 1, false);


--
-- TOC entry 4867 (class 2606 OID 16739)
-- Name: inmueble inmueble_pkey; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.inmueble
    ADD CONSTRAINT inmueble_pkey PRIMARY KEY (id);


--
-- TOC entry 4865 (class 2606 OID 16723)
-- Name: propietario propietario_pkey; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.propietario
    ADD CONSTRAINT propietario_pkey PRIMARY KEY (id);


--
-- Name: usuario usuario_pkey; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.usuario
    ADD CONSTRAINT usuario_pkey PRIMARY KEY (id_usuario);


--
-- Name: usuario usuario_email_key; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.usuario
    ADD CONSTRAINT usuario_email_key UNIQUE (email);


--
-- Name: reserva reserva_pkey; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.reserva
    ADD CONSTRAINT reserva_pkey PRIMARY KEY (id);


--
-- Name: pago pago_pkey; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.pago
    ADD CONSTRAINT pago_pkey PRIMARY KEY (id);

--
-- Name: pago pago_metodo_check; Type: CHECK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.pago
    ADD CONSTRAINT pago_metodo_check CHECK (((metodo)::text = ANY ((ARRAY['Efectivo'::character varying, 'Transferencia'::character varying, 'Tarjeta'::character varying, 'MercadoPago'::character varying])::text[])));

--
-- Name: pago pago_estado_check; Type: CHECK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.pago
    ADD CONSTRAINT pago_estado_check CHECK (((estado)::text = ANY ((ARRAY['Pendiente'::character varying, 'Pagado'::character varying, 'Anulado'::character varying, 'Rechazado'::character varying])::text[])));


--
-- TOC entry 4868 (class 2606 OID 16740)
-- Name: inmueble inmueble_propietario_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.inmueble
    ADD CONSTRAINT inmueble_propietario_id_fkey FOREIGN KEY (propietario_id) REFERENCES public.propietario(id);


--
-- Name: reserva reserva_usuario_creador_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.reserva
    ADD CONSTRAINT reserva_usuario_creador_id_fkey FOREIGN KEY (usuario_creador_id) REFERENCES public.usuario(id_usuario);


--
-- Name: reserva reserva_usuario_terminador_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.reserva
    ADD CONSTRAINT reserva_usuario_terminador_id_fkey FOREIGN KEY (usuario_terminador_id) REFERENCES public.usuario(id_usuario);


--
-- Name: pago pago_reserva_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.pago
    ADD CONSTRAINT pago_reserva_id_fkey FOREIGN KEY (reserva_id) REFERENCES public.reserva(id);


--
-- Name: pago pago_usuario_creador_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.pago
    ADD CONSTRAINT pago_usuario_creador_id_fkey FOREIGN KEY (usuario_creador_id) REFERENCES public.usuario(id_usuario);


--
-- Name: pago pago_usuario_anulador_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.pago
    ADD CONSTRAINT pago_usuario_anulador_id_fkey FOREIGN KEY (usuario_anulador_id) REFERENCES public.usuario(id_usuario);


-- Completed on 2026-08-20 19:49:08

--
-- PostgreSQL database dump complete
--

\unrestrict QtSBGeRRY6j6Jwo1fkh8gtkMmOFbsIZaIOddumQrOTNweMS0szVWYyfIOxsVWS0
