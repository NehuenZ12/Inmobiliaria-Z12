--
-- PostgreSQL database dump
--

\restrict vcEW8wtblpEPUo1iyEInl83B5GL6CMXj4rlXZO6fkLy6eCXcRVhhTuEqtgOjRbp

-- Dumped from database version 18.4
-- Dumped by pg_dump version 18.4

-- Started on 2026-09-17 20:32:31

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
-- TOC entry 225 (class 1259 OID 16774)
-- Name: imagen; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public.imagen (
    id integer NOT NULL,
    url character varying(500) NOT NULL,
    descripcion character varying(255),
    es_principal boolean DEFAULT false NOT NULL,
    inmueble_id integer NOT NULL
);


--
-- TOC entry 226 (class 1259 OID 16784)
-- Name: imagen_id_seq; Type: SEQUENCE; Schema: public; Owner: -
--

CREATE SEQUENCE public.imagen_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


--
-- TOC entry 5111 (class 0 OID 0)
-- Dependencies: 226
-- Name: imagen_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: -
--

ALTER SEQUENCE public.imagen_id_seq OWNED BY public.imagen.id;


--
-- TOC entry 222 (class 1259 OID 16725)
-- Name: inmueble; Type: TABLE; Schema: public; Owner: -
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
    propietario_id integer NOT NULL,
    tipo_id integer NOT NULL,
    descripcion character varying(500)
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
-- TOC entry 5112 (class 0 OID 0)
-- Dependencies: 221
-- Name: inmueble_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: -
--

ALTER SEQUENCE public.inmueble_id_seq OWNED BY public.inmueble.id;


--
-- TOC entry 229 (class 1259 OID 16815)
-- Name: inquilino; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public.inquilino (
    id integer NOT NULL,
    nombre character varying(100) NOT NULL,
    apellido character varying(100) NOT NULL,
    dni character varying(20) NOT NULL,
    telefono character varying(30),
    email character varying(150),
    fecha_alta timestamp without time zone DEFAULT now() NOT NULL
);


--
-- TOC entry 230 (class 1259 OID 16824)
-- Name: inquilino_id_seq; Type: SEQUENCE; Schema: public; Owner: -
--

CREATE SEQUENCE public.inquilino_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


--
-- TOC entry 5113 (class 0 OID 0)
-- Dependencies: 230
-- Name: inquilino_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: -
--

ALTER SEQUENCE public.inquilino_id_seq OWNED BY public.inquilino.id;


--
-- TOC entry 233 (class 1259 OID 16869)
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
    comprobante_url character varying(500),
    CONSTRAINT pago_estado_check CHECK (((estado)::text = ANY (ARRAY[('Pendiente'::character varying)::text, ('Pagado'::character varying)::text, ('Anulado'::character varying)::text, ('Rechazado'::character varying)::text]))),
    CONSTRAINT pago_metodo_check CHECK (((metodo)::text = ANY (ARRAY[('Efectivo'::character varying)::text, ('Transferencia'::character varying)::text, ('Tarjeta'::character varying)::text, ('MercadoPago'::character varying)::text])))
);


--
-- TOC entry 234 (class 1259 OID 16887)
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
-- TOC entry 5114 (class 0 OID 0)
-- Dependencies: 234
-- Name: pago_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: -
--

ALTER SEQUENCE public.pago_id_seq OWNED BY public.pago.id;


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
-- TOC entry 5115 (class 0 OID 0)
-- Dependencies: 219
-- Name: propietario_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: -
--

ALTER SEQUENCE public.propietario_id_seq OWNED BY public.propietario.id;


--
-- TOC entry 231 (class 1259 OID 16828)
-- Name: reserva; Type: TABLE; Schema: public; Owner: -
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
    fecha_creacion timestamp without time zone DEFAULT now() NOT NULL,
    fecha_terminacion date,
    CONSTRAINT reserva_estado_check CHECK (((estado)::text = ANY (ARRAY[('Pendiente'::character varying)::text, ('Confirmada'::character varying)::text, ('Cancelada'::character varying)::text, ('Completada'::character varying)::text, ('Expirada'::character varying)::text]))),
    CONSTRAINT reserva_fechas_check CHECK ((fecha_hasta > fecha_desde))
);


--
-- TOC entry 232 (class 1259 OID 16845)
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
-- TOC entry 5116 (class 0 OID 0)
-- Dependencies: 232
-- Name: reserva_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: -
--

ALTER SEQUENCE public.reserva_id_seq OWNED BY public.reserva.id;


--
-- TOC entry 223 (class 1259 OID 16755)
-- Name: tipo; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public.tipo (
    id integer NOT NULL,
    nombre character varying(50) NOT NULL,
    descripcion character varying(255)
);


--
-- TOC entry 224 (class 1259 OID 16760)
-- Name: tipo_id_seq; Type: SEQUENCE; Schema: public; Owner: -
--

CREATE SEQUENCE public.tipo_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


--
-- TOC entry 5117 (class 0 OID 0)
-- Dependencies: 224
-- Name: tipo_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: -
--

ALTER SEQUENCE public.tipo_id_seq OWNED BY public.tipo.id;


--
-- TOC entry 227 (class 1259 OID 16793)
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
    activo boolean DEFAULT true NOT NULL,
    fecha_alta timestamp without time zone DEFAULT now() NOT NULL,
    CONSTRAINT usuario_rol_check CHECK (((rol)::text = ANY (ARRAY[('Administrador'::character varying)::text, ('Empleado'::character varying)::text])))
);


--
-- TOC entry 228 (class 1259 OID 16809)
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
-- TOC entry 5118 (class 0 OID 0)
-- Dependencies: 228
-- Name: usuario_id_usuario_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: -
--

ALTER SEQUENCE public.usuario_id_usuario_seq OWNED BY public.usuario.id_usuario;


--
-- TOC entry 4895 (class 2604 OID 16785)
-- Name: imagen id; Type: DEFAULT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.imagen ALTER COLUMN id SET DEFAULT nextval('public.imagen_id_seq'::regclass);


--
-- TOC entry 4892 (class 2604 OID 16728)
-- Name: inmueble id; Type: DEFAULT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.inmueble ALTER COLUMN id SET DEFAULT nextval('public.inmueble_id_seq'::regclass);


--
-- TOC entry 4900 (class 2604 OID 16825)
-- Name: inquilino id; Type: DEFAULT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.inquilino ALTER COLUMN id SET DEFAULT nextval('public.inquilino_id_seq'::regclass);


--
-- TOC entry 4905 (class 2604 OID 16888)
-- Name: pago id; Type: DEFAULT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.pago ALTER COLUMN id SET DEFAULT nextval('public.pago_id_seq'::regclass);


--
-- TOC entry 4891 (class 2604 OID 16717)
-- Name: propietario id; Type: DEFAULT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.propietario ALTER COLUMN id SET DEFAULT nextval('public.propietario_id_seq'::regclass);


--
-- TOC entry 4902 (class 2604 OID 16846)
-- Name: reserva id; Type: DEFAULT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.reserva ALTER COLUMN id SET DEFAULT nextval('public.reserva_id_seq'::regclass);


--
-- TOC entry 4894 (class 2604 OID 16761)
-- Name: tipo id; Type: DEFAULT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.tipo ALTER COLUMN id SET DEFAULT nextval('public.tipo_id_seq'::regclass);


--
-- TOC entry 4897 (class 2604 OID 16810)
-- Name: usuario id_usuario; Type: DEFAULT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.usuario ALTER COLUMN id_usuario SET DEFAULT nextval('public.usuario_id_usuario_seq'::regclass);


--
-- TOC entry 5096 (class 0 OID 16774)
-- Dependencies: 225
-- Data for Name: imagen; Type: TABLE DATA; Schema: public; Owner: -
--

INSERT INTO public.imagen VALUES (1, '/uploads/inmuebles/743ce105-5538-4ac9-bbaa-0ed1c20a48c1.jpeg', 'casa1.jpeg', true, 5);
INSERT INTO public.imagen VALUES (2, '/uploads/inmuebles/fa9227f2-c922-4bb4-9eca-f1aaca1444dc.jpg', 'casa2.jpg', true, 6);
INSERT INTO public.imagen VALUES (3, '/uploads/inmuebles/0e077297-d945-4cad-b5dd-7875e3637f74.jpg', 'casa3.jpg', false, 6);
INSERT INTO public.imagen VALUES (4, '/uploads/inmuebles/21bc72fa-78b9-4217-9795-39bd36193e8f.jpg', 'casa55.jpg', true, 7);
INSERT INTO public.imagen VALUES (5, '/uploads/inmuebles/d79e4f0e-73b2-4033-b34c-a87f4c7ebd8e.jpg', 'casa555.jpg', false, 7);
INSERT INTO public.imagen VALUES (6, '/uploads/inmuebles/773ee755-ed76-4ce1-89dc-ecbad6f994a9.jpg', 'casa666.jpg', true, 8);


--
-- TOC entry 5093 (class 0 OID 16725)
-- Dependencies: 222
-- Data for Name: inmueble; Type: TABLE DATA; Schema: public; Owner: -
--

INSERT INTO public.inmueble VALUES (1, '25 de mayo 950 San Luis', 2, 20.0000000, 15.0000000, 50.00, 50.00, true, 1, 1, NULL);
INSERT INTO public.inmueble VALUES (3, 'Sarmiento 532 San Luis', 2, 10.0000000, 5.0000000, 25.00, 30.00, true, 3, 2, NULL);
INSERT INTO public.inmueble VALUES (4, 'Av centenario 245 San Luis', 3, 30.0000000, 20.0000000, 45.00, 50.00, true, 2, 1, NULL);
INSERT INTO public.inmueble VALUES (5, 'Pedernera 503', 2, 40.0000000, 30.0000000, 1000.00, 50.00, true, 1, 1, NULL);
INSERT INTO public.inmueble VALUES (6, 'Junin 211', 3, 55.0000000, 36.0000000, 1500.00, 50.00, true, 1, 1, NULL);
INSERT INTO public.inmueble VALUES (7, 'Maipu 720', 3, 33.0000000, 22.0000000, 2500.00, 50.00, true, 6, 3, NULL);
INSERT INTO public.inmueble VALUES (8, 'Europa 503', 2, 44.0000000, 22.0000000, 1100.00, 55.00, false, 5, 1, NULL);


--
-- TOC entry 5100 (class 0 OID 16815)
-- Dependencies: 229
-- Data for Name: inquilino; Type: TABLE DATA; Schema: public; Owner: -
--



--
-- TOC entry 5104 (class 0 OID 16869)
-- Dependencies: 233
-- Data for Name: pago; Type: TABLE DATA; Schema: public; Owner: -
--



--
-- TOC entry 5091 (class 0 OID 16714)
-- Dependencies: 220
-- Data for Name: propietario; Type: TABLE DATA; Schema: public; Owner: -
--

INSERT INTO public.propietario VALUES (2, 'Heber', 'Gomez', '40123321', '2664995566', 'hebergomez@gmail.com');
INSERT INTO public.propietario VALUES (3, 'Jose', 'Garces', '39098123', '2664774455', 'josegarces@gmail.com');
INSERT INTO public.propietario VALUES (1, 'Nehuen ', 'Ferrero', '42123123', '2657111111', 'nehuen123@gmail.com');
INSERT INTO public.propietario VALUES (5, 'Juan', 'Flores', '4223121', '2665002211', 'juan@gmail.com');
INSERT INTO public.propietario VALUES (6, 'Pedro', 'Perez', '1231233', '222333444', 'pedro123@gmail.com');
INSERT INTO public.propietario VALUES (7, 'Roberto', 'Gomez', '12222335', '244456789', 'roberto@gmail.com');


--
-- TOC entry 5102 (class 0 OID 16828)
-- Dependencies: 231
-- Data for Name: reserva; Type: TABLE DATA; Schema: public; Owner: -
--



--
-- TOC entry 5094 (class 0 OID 16755)
-- Dependencies: 223
-- Data for Name: tipo; Type: TABLE DATA; Schema: public; Owner: -
--

INSERT INTO public.tipo VALUES (1, 'Casa', NULL);
INSERT INTO public.tipo VALUES (2, 'Monoambiente', NULL);
INSERT INTO public.tipo VALUES (3, 'Departamento', NULL);
INSERT INTO public.tipo VALUES (5, 'Local', NULL);
INSERT INTO public.tipo VALUES (6, 'Oficina', NULL);
INSERT INTO public.tipo VALUES (7, 'Terreno', NULL);
INSERT INTO public.tipo VALUES (4, 'Cabaña', NULL);
INSERT INTO public.tipo VALUES (10, 'Terrenos', NULL);


--
-- TOC entry 5098 (class 0 OID 16793)
-- Dependencies: 227
-- Data for Name: usuario; Type: TABLE DATA; Schema: public; Owner: -
--

INSERT INTO public.usuario VALUES (1, 'Admin', 'Sistema', 'admin@admin.com', 'AQAAAAIAAYagAAAAELbGDnHFw8juHh4fg+Vn3vAEOQs2Y06K/P56MAnNAfS3M81vgrx1b5IxI1YZxYKqUw==', NULL, 'Administrador', true, '2026-09-04 14:55:46.785021');
INSERT INTO public.usuario VALUES (2, 'nehuen', 'zerda', 'nehuen123@gmail.com', 'AQAAAAIAAYagAAAAEPWQ3tU3NQfGI3/rRI8YeV6En++Mu2Oemp6MFxkXyu9wMHmxvilWDZvxFrWk9MmRmg==', NULL, 'Empleado', true, '2026-09-17 17:20:39.654612');


--
-- TOC entry 5119 (class 0 OID 0)
-- Dependencies: 226
-- Name: imagen_id_seq; Type: SEQUENCE SET; Schema: public; Owner: -
--

SELECT pg_catalog.setval('public.imagen_id_seq', 6, true);


--
-- TOC entry 5120 (class 0 OID 0)
-- Dependencies: 221
-- Name: inmueble_id_seq; Type: SEQUENCE SET; Schema: public; Owner: -
--

SELECT pg_catalog.setval('public.inmueble_id_seq', 8, true);


--
-- TOC entry 5121 (class 0 OID 0)
-- Dependencies: 230
-- Name: inquilino_id_seq; Type: SEQUENCE SET; Schema: public; Owner: -
--

SELECT pg_catalog.setval('public.inquilino_id_seq', 1, false);


--
-- TOC entry 5122 (class 0 OID 0)
-- Dependencies: 234
-- Name: pago_id_seq; Type: SEQUENCE SET; Schema: public; Owner: -
--

SELECT pg_catalog.setval('public.pago_id_seq', 1, false);


--
-- TOC entry 5123 (class 0 OID 0)
-- Dependencies: 219
-- Name: propietario_id_seq; Type: SEQUENCE SET; Schema: public; Owner: -
--

SELECT pg_catalog.setval('public.propietario_id_seq', 7, true);


--
-- TOC entry 5124 (class 0 OID 0)
-- Dependencies: 232
-- Name: reserva_id_seq; Type: SEQUENCE SET; Schema: public; Owner: -
--

SELECT pg_catalog.setval('public.reserva_id_seq', 1, false);


--
-- TOC entry 5125 (class 0 OID 0)
-- Dependencies: 224
-- Name: tipo_id_seq; Type: SEQUENCE SET; Schema: public; Owner: -
--

SELECT pg_catalog.setval('public.tipo_id_seq', 10, true);


--
-- TOC entry 5126 (class 0 OID 0)
-- Dependencies: 228
-- Name: usuario_id_usuario_seq; Type: SEQUENCE SET; Schema: public; Owner: -
--

SELECT pg_catalog.setval('public.usuario_id_usuario_seq', 2, true);


--
-- TOC entry 4922 (class 2606 OID 16787)
-- Name: imagen imagen_pkey; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.imagen
    ADD CONSTRAINT imagen_pkey PRIMARY KEY (id);


--
-- TOC entry 4916 (class 2606 OID 16739)
-- Name: inmueble inmueble_pkey; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.inmueble
    ADD CONSTRAINT inmueble_pkey PRIMARY KEY (id);


--
-- TOC entry 4928 (class 2606 OID 16827)
-- Name: inquilino inquilino_pkey; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.inquilino
    ADD CONSTRAINT inquilino_pkey PRIMARY KEY (id);


--
-- TOC entry 4932 (class 2606 OID 16890)
-- Name: pago pago_pkey; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.pago
    ADD CONSTRAINT pago_pkey PRIMARY KEY (id);


--
-- TOC entry 4914 (class 2606 OID 16723)
-- Name: propietario propietario_pkey; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.propietario
    ADD CONSTRAINT propietario_pkey PRIMARY KEY (id);


--
-- TOC entry 4930 (class 2606 OID 16848)
-- Name: reserva reserva_pkey; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.reserva
    ADD CONSTRAINT reserva_pkey PRIMARY KEY (id);


--
-- TOC entry 4918 (class 2606 OID 16765)
-- Name: tipo tipo_nombre_key; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.tipo
    ADD CONSTRAINT tipo_nombre_key UNIQUE (nombre);


--
-- TOC entry 4920 (class 2606 OID 16763)
-- Name: tipo tipo_pkey; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.tipo
    ADD CONSTRAINT tipo_pkey PRIMARY KEY (id);


--
-- TOC entry 4924 (class 2606 OID 16814)
-- Name: usuario usuario_email_key; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.usuario
    ADD CONSTRAINT usuario_email_key UNIQUE (email);


--
-- TOC entry 4926 (class 2606 OID 16812)
-- Name: usuario usuario_pkey; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.usuario
    ADD CONSTRAINT usuario_pkey PRIMARY KEY (id_usuario);


--
-- TOC entry 4935 (class 2606 OID 16788)
-- Name: imagen imagen_inmueble_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.imagen
    ADD CONSTRAINT imagen_inmueble_id_fkey FOREIGN KEY (inmueble_id) REFERENCES public.inmueble(id);


--
-- TOC entry 4933 (class 2606 OID 16740)
-- Name: inmueble inmueble_propietario_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.inmueble
    ADD CONSTRAINT inmueble_propietario_id_fkey FOREIGN KEY (propietario_id) REFERENCES public.propietario(id);


--
-- TOC entry 4934 (class 2606 OID 16769)
-- Name: inmueble inmueble_tipo_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.inmueble
    ADD CONSTRAINT inmueble_tipo_id_fkey FOREIGN KEY (tipo_id) REFERENCES public.tipo(id);


--
-- TOC entry 4940 (class 2606 OID 16891)
-- Name: pago pago_reserva_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.pago
    ADD CONSTRAINT pago_reserva_id_fkey FOREIGN KEY (reserva_id) REFERENCES public.reserva(id);


--
-- TOC entry 4941 (class 2606 OID 16901)
-- Name: pago pago_usuario_anulador_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.pago
    ADD CONSTRAINT pago_usuario_anulador_id_fkey FOREIGN KEY (usuario_anulador_id) REFERENCES public.usuario(id_usuario);


--
-- TOC entry 4942 (class 2606 OID 16896)
-- Name: pago pago_usuario_creador_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.pago
    ADD CONSTRAINT pago_usuario_creador_id_fkey FOREIGN KEY (usuario_creador_id) REFERENCES public.usuario(id_usuario);


--
-- TOC entry 4936 (class 2606 OID 16854)
-- Name: reserva reserva_inmueble_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.reserva
    ADD CONSTRAINT reserva_inmueble_id_fkey FOREIGN KEY (inmueble_id) REFERENCES public.inmueble(id);


--
-- TOC entry 4937 (class 2606 OID 16849)
-- Name: reserva reserva_inquilino_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.reserva
    ADD CONSTRAINT reserva_inquilino_id_fkey FOREIGN KEY (inquilino_id) REFERENCES public.inquilino(id);


--
-- TOC entry 4938 (class 2606 OID 16859)
-- Name: reserva reserva_usuario_creador_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.reserva
    ADD CONSTRAINT reserva_usuario_creador_id_fkey FOREIGN KEY (usuario_creador_id) REFERENCES public.usuario(id_usuario);


--
-- TOC entry 4939 (class 2606 OID 16864)
-- Name: reserva reserva_usuario_terminador_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.reserva
    ADD CONSTRAINT reserva_usuario_terminador_id_fkey FOREIGN KEY (usuario_terminador_id) REFERENCES public.usuario(id_usuario);


-- Completed on 2026-09-17 20:32:31

--
-- PostgreSQL database dump complete
--

\unrestrict vcEW8wtblpEPUo1iyEInl83B5GL6CMXj4rlXZO6fkLy6eCXcRVhhTuEqtgOjRbp

