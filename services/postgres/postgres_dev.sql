--
-- PostgreSQL database dump
--

-- Dumped from database version 9.4.9
-- Dumped by pg_dump version 9.5.4

-- Started on 2016-09-21 15:38:40

SET statement_timeout = 0;
SET lock_timeout = 0;
SET client_encoding = 'UTF8';
SET standard_conforming_strings = on;
SET check_function_bodies = false;
SET client_min_messages = warning;
SET row_security = off;

--
-- TOC entry 2063 (class 1262 OID 12141)
-- Dependencies: 2062
-- Name: postgres; Type: COMMENT; Schema: -; Owner: postgres
--

COMMENT ON DATABASE postgres IS 'default administrative connection database';


--
-- TOC entry 2 (class 3079 OID 11861)
-- Name: plpgsql; Type: EXTENSION; Schema: -; Owner: 
--

CREATE EXTENSION IF NOT EXISTS plpgsql WITH SCHEMA pg_catalog;


--
-- TOC entry 2066 (class 0 OID 0)
-- Dependencies: 2
-- Name: EXTENSION plpgsql; Type: COMMENT; Schema: -; Owner: 
--

COMMENT ON EXTENSION plpgsql IS 'PL/pgSQL procedural language';


--
-- TOC entry 1 (class 3079 OID 16384)
-- Name: adminpack; Type: EXTENSION; Schema: -; Owner: 
--

CREATE EXTENSION IF NOT EXISTS adminpack WITH SCHEMA pg_catalog;


--
-- TOC entry 2067 (class 0 OID 0)
-- Dependencies: 1
-- Name: EXTENSION adminpack; Type: COMMENT; Schema: -; Owner: 
--

COMMENT ON EXTENSION adminpack IS 'administrative functions for PostgreSQL';


SET search_path = public, pg_catalog;

SET default_tablespace = '';

SET default_with_oids = false;

--
-- TOC entry 174 (class 1259 OID 16393)
-- Name: activity; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE activity (
    id integer NOT NULL,
    ip character varying,
    username character varying,
    request character varying,
    date date DEFAULT now(),
    "timestamp" timestamp without time zone DEFAULT now()
);


ALTER TABLE activity OWNER TO postgres;

--
-- TOC entry 175 (class 1259 OID 16400)
-- Name: activity_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE activity_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE activity_id_seq OWNER TO postgres;

--
-- TOC entry 2068 (class 0 OID 0)
-- Dependencies: 175
-- Name: activity_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE activity_id_seq OWNED BY activity.id;


--
-- TOC entry 176 (class 1259 OID 16402)
-- Name: exceptions; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE exceptions (
    id integer NOT NULL,
    date date DEFAULT now() NOT NULL,
    criticality smallint DEFAULT 1 NOT NULL,
    message character varying,
    stacktrace character varying,
    "time" timestamp without time zone DEFAULT now(),
    query character varying
);


ALTER TABLE exceptions OWNER TO postgres;

--
-- TOC entry 177 (class 1259 OID 16410)
-- Name: exceptions_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE exceptions_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE exceptions_id_seq OWNER TO postgres;

--
-- TOC entry 2069 (class 0 OID 0)
-- Dependencies: 177
-- Name: exceptions_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE exceptions_id_seq OWNED BY exceptions.id;


--
-- TOC entry 178 (class 1259 OID 16412)
-- Name: itemhistory; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE itemhistory (
    id integer NOT NULL,
    itemid character varying NOT NULL,
    seller character varying DEFAULT 'none'::character varying NOT NULL,
    date date DEFAULT now() NOT NULL,
    meta jsonb,
    price numeric[] NOT NULL,
    quantity integer DEFAULT 1 NOT NULL,
    lotprice numeric DEFAULT 0 NOT NULL,
    currency character varying,
    title character varying,
    source character varying
);


ALTER TABLE itemhistory OWNER TO postgres;

--
-- TOC entry 179 (class 1259 OID 16422)
-- Name: itemhistory_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE itemhistory_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE itemhistory_id_seq OWNER TO postgres;

--
-- TOC entry 2070 (class 0 OID 0)
-- Dependencies: 179
-- Name: itemhistory_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE itemhistory_id_seq OWNED BY itemhistory.id;


--
-- TOC entry 180 (class 1259 OID 16424)
-- Name: searchhistory; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE searchhistory (
    id integer NOT NULL,
    search character varying(250) NOT NULL,
    date date DEFAULT now() NOT NULL,
    meta jsonb,
    username character varying(80) NOT NULL,
    "timestamp" timestamp without time zone DEFAULT now()
);


ALTER TABLE searchhistory OWNER TO postgres;

--
-- TOC entry 181 (class 1259 OID 16431)
-- Name: searchhistory_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE searchhistory_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE searchhistory_id_seq OWNER TO postgres;

--
-- TOC entry 2071 (class 0 OID 0)
-- Dependencies: 181
-- Name: searchhistory_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE searchhistory_id_seq OWNED BY searchhistory.id;


--
-- TOC entry 182 (class 1259 OID 16433)
-- Name: users; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE users (
    id integer NOT NULL,
    username character varying(80) NOT NULL,
    email character varying(250) NOT NULL,
    password character varying(128) NOT NULL,
    salt character varying(128) NOT NULL,
    meta jsonb,
    created_date date DEFAULT now() NOT NULL,
    reset character varying
);


ALTER TABLE users OWNER TO postgres;

--
-- TOC entry 183 (class 1259 OID 16440)
-- Name: users_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE users_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE users_id_seq OWNER TO postgres;

--
-- TOC entry 2072 (class 0 OID 0)
-- Dependencies: 183
-- Name: users_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE users_id_seq OWNED BY users.id;


--
-- TOC entry 1916 (class 2604 OID 16442)
-- Name: id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY activity ALTER COLUMN id SET DEFAULT nextval('activity_id_seq'::regclass);


--
-- TOC entry 1920 (class 2604 OID 16443)
-- Name: id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY exceptions ALTER COLUMN id SET DEFAULT nextval('exceptions_id_seq'::regclass);


--
-- TOC entry 1926 (class 2604 OID 16444)
-- Name: id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY itemhistory ALTER COLUMN id SET DEFAULT nextval('itemhistory_id_seq'::regclass);


--
-- TOC entry 1928 (class 2604 OID 16445)
-- Name: id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY searchhistory ALTER COLUMN id SET DEFAULT nextval('searchhistory_id_seq'::regclass);


--
-- TOC entry 1931 (class 2604 OID 16446)
-- Name: id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY users ALTER COLUMN id SET DEFAULT nextval('users_id_seq'::regclass);


--
-- TOC entry 1934 (class 2606 OID 16448)
-- Name: activity_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY activity
    ADD CONSTRAINT activity_pkey PRIMARY KEY (id);


--
-- TOC entry 1937 (class 2606 OID 16450)
-- Name: exceptions_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY exceptions
    ADD CONSTRAINT exceptions_pkey PRIMARY KEY (id);


--
-- TOC entry 1940 (class 2606 OID 16452)
-- Name: itemhistory_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY itemhistory
    ADD CONSTRAINT itemhistory_pkey PRIMARY KEY (id);


--
-- TOC entry 1943 (class 2606 OID 16454)
-- Name: searchhistory_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY searchhistory
    ADD CONSTRAINT searchhistory_pkey PRIMARY KEY (id);


--
-- TOC entry 1947 (class 2606 OID 16456)
-- Name: users_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY users
    ADD CONSTRAINT users_pkey PRIMARY KEY (id);


--
-- TOC entry 1932 (class 1259 OID 16457)
-- Name: activity_id_uindex; Type: INDEX; Schema: public; Owner: postgres
--

CREATE UNIQUE INDEX activity_id_uindex ON activity USING btree (id);


--
-- TOC entry 1935 (class 1259 OID 16458)
-- Name: exceptions_id_uindex; Type: INDEX; Schema: public; Owner: postgres
--

CREATE UNIQUE INDEX exceptions_id_uindex ON exceptions USING btree (id);


--
-- TOC entry 1938 (class 1259 OID 16459)
-- Name: itemhistory_id_uindex; Type: INDEX; Schema: public; Owner: postgres
--

CREATE UNIQUE INDEX itemhistory_id_uindex ON itemhistory USING btree (id);


--
-- TOC entry 1941 (class 1259 OID 16460)
-- Name: searchhistory_id_uindex; Type: INDEX; Schema: public; Owner: postgres
--

CREATE UNIQUE INDEX searchhistory_id_uindex ON searchhistory USING btree (id);


--
-- TOC entry 1944 (class 1259 OID 16461)
-- Name: users_email_uindex; Type: INDEX; Schema: public; Owner: postgres
--

CREATE UNIQUE INDEX users_email_uindex ON users USING btree (email);


--
-- TOC entry 1945 (class 1259 OID 16462)
-- Name: users_id_uindex; Type: INDEX; Schema: public; Owner: postgres
--

CREATE UNIQUE INDEX users_id_uindex ON users USING btree (id);


--
-- TOC entry 1948 (class 1259 OID 16463)
-- Name: users_useranme_uindex; Type: INDEX; Schema: public; Owner: postgres
--

CREATE UNIQUE INDEX users_useranme_uindex ON users USING btree (username);


--
-- TOC entry 2065 (class 0 OID 0)
-- Dependencies: 8
-- Name: public; Type: ACL; Schema: -; Owner: postgres
--

REVOKE ALL ON SCHEMA public FROM PUBLIC;
REVOKE ALL ON SCHEMA public FROM postgres;
GRANT ALL ON SCHEMA public TO postgres;
GRANT ALL ON SCHEMA public TO PUBLIC;


-- Completed on 2016-09-21 15:38:48

--
-- PostgreSQL database dump complete
--

