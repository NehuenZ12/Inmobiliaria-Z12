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
-- TOC entry 4868 (class 2606 OID 16740)
-- Name: inmueble inmueble_propietario_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.inmueble
    ADD CONSTRAINT inmueble_propietario_id_fkey FOREIGN KEY (propietario_id) REFERENCES public.propietario(id);


-- Completed on 2026-08-20 19:49:08
--
-- Name: inquilino; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public.inquilino (
    id integer NOT NULL,
    nombre character varying(100) NOT NULL,
    apellido character varying(100) NOT NULL,
    dni character varying(20) NOT NULL,
    telefono character varying(30),
    email character varying(150)
);

--
-- Name: inquilino_id_seq; Type: SEQUENCE; Schema: public; Owner: -
--

CREATE SEQUENCE public.inquilino_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;

ALTER SEQUENCE public.inquilino_id_seq OWNED BY public.inquilino.id;

ALTER TABLE ONLY public.inquilino ALTER COLUMN id SET DEFAULT nextval('public.inquilino_id_seq'::regclass);

ALTER TABLE ONLY public.inquilino
    ADD CONSTRAINT inquilino_pkey PRIMARY KEY (id);
--
-- PostgreSQL database dump complete
--

\unrestrict QtSBGeRRY6j6Jwo1fkh8gtkMmOFbsIZaIOddumQrOTNweMS0szVWYyfIOxsVWS0

