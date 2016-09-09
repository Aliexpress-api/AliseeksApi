--
-- PostgreSQL database dump
--

-- Dumped from database version 9.5.4
-- Dumped by pg_dump version 9.5.4

-- Started on 2016-09-08 16:28:58

SET statement_timeout = 0;
SET lock_timeout = 0;
SET client_encoding = 'UTF8';
SET standard_conforming_strings = on;
SET check_function_bodies = false;
SET client_min_messages = warning;

--
-- TOC entry 2136 (class 1262 OID 12373)
-- Dependencies: 2135
-- Name: postgres; Type: COMMENT; Schema: -; Owner: postgres
--

COMMENT ON DATABASE postgres IS 'default administrative connection database';


--
-- TOC entry 2 (class 3079 OID 12355)
-- Name: plpgsql; Type: EXTENSION; Schema: -; Owner: 
--

CREATE EXTENSION IF NOT EXISTS plpgsql WITH SCHEMA pg_catalog;


--
-- TOC entry 2139 (class 0 OID 0)
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
-- TOC entry 2140 (class 0 OID 0)
-- Dependencies: 1
-- Name: EXTENSION adminpack; Type: COMMENT; Schema: -; Owner: 
--

COMMENT ON EXTENSION adminpack IS 'administrative functions for PostgreSQL';


SET search_path = public, pg_catalog;

SET default_tablespace = '';

SET default_with_oids = false;

--
-- TOC entry 187 (class 1259 OID 16420)
-- Name: itemhistory; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE itemhistory (
    id integer NOT NULL,
    itemid character varying NOT NULL,
    price numeric DEFAULT 0 NOT NULL,
    quantity smallint DEFAULT 1 NOT NULL,
    seller character varying DEFAULT 'none'::character varying NOT NULL,
    date date DEFAULT now() NOT NULL,
    meta jsonb
);


ALTER TABLE itemhistory OWNER TO postgres;

--
-- TOC entry 186 (class 1259 OID 16418)
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
-- TOC entry 2141 (class 0 OID 0)
-- Dependencies: 186
-- Name: itemhistory_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE itemhistory_id_seq OWNED BY itemhistory.id;


--
-- TOC entry 185 (class 1259 OID 16410)
-- Name: searchhistory; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE searchhistory (
    id integer NOT NULL,
    search character varying(250) NOT NULL,
    date date DEFAULT now() NOT NULL,
    meta jsonb,
    username character varying(80) NOT NULL
);


ALTER TABLE searchhistory OWNER TO postgres;

--
-- TOC entry 184 (class 1259 OID 16408)
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
-- TOC entry 2142 (class 0 OID 0)
-- Dependencies: 184
-- Name: searchhistory_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE searchhistory_id_seq OWNED BY searchhistory.id;


--
-- TOC entry 183 (class 1259 OID 16395)
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
-- TOC entry 182 (class 1259 OID 16393)
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
-- TOC entry 2143 (class 0 OID 0)
-- Dependencies: 182
-- Name: users_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE users_id_seq OWNED BY users.id;


--
-- TOC entry 2001 (class 2604 OID 16423)
-- Name: id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY itemhistory ALTER COLUMN id SET DEFAULT nextval('itemhistory_id_seq'::regclass);


--
-- TOC entry 1999 (class 2604 OID 16413)
-- Name: id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY searchhistory ALTER COLUMN id SET DEFAULT nextval('searchhistory_id_seq'::regclass);


--
-- TOC entry 1997 (class 2604 OID 16398)
-- Name: id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY users ALTER COLUMN id SET DEFAULT nextval('users_id_seq'::regclass);


--
-- TOC entry 2016 (class 2606 OID 16432)
-- Name: itemhistory_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY itemhistory
    ADD CONSTRAINT itemhistory_pkey PRIMARY KEY (id);


--
-- TOC entry 2013 (class 2606 OID 16416)
-- Name: searchhistory_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY searchhistory
    ADD CONSTRAINT searchhistory_pkey PRIMARY KEY (id);


--
-- TOC entry 2009 (class 2606 OID 16404)
-- Name: users_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY users
    ADD CONSTRAINT users_pkey PRIMARY KEY (id);


--
-- TOC entry 2014 (class 1259 OID 16433)
-- Name: itemhistory_id_uindex; Type: INDEX; Schema: public; Owner: postgres
--

CREATE UNIQUE INDEX itemhistory_id_uindex ON itemhistory USING btree (id);


--
-- TOC entry 2011 (class 1259 OID 16417)
-- Name: searchhistory_id_uindex; Type: INDEX; Schema: public; Owner: postgres
--

CREATE UNIQUE INDEX searchhistory_id_uindex ON searchhistory USING btree (id);


--
-- TOC entry 2006 (class 1259 OID 16407)
-- Name: users_email_uindex; Type: INDEX; Schema: public; Owner: postgres
--

CREATE UNIQUE INDEX users_email_uindex ON users USING btree (email);


--
-- TOC entry 2007 (class 1259 OID 16405)
-- Name: users_id_uindex; Type: INDEX; Schema: public; Owner: postgres
--

CREATE UNIQUE INDEX users_id_uindex ON users USING btree (id);


--
-- TOC entry 2010 (class 1259 OID 16406)
-- Name: users_useranme_uindex; Type: INDEX; Schema: public; Owner: postgres
--

CREATE UNIQUE INDEX users_useranme_uindex ON users USING btree (username);


--
-- TOC entry 2138 (class 0 OID 0)
-- Dependencies: 7
-- Name: public; Type: ACL; Schema: -; Owner: postgres
--

REVOKE ALL ON SCHEMA public FROM PUBLIC;
REVOKE ALL ON SCHEMA public FROM postgres;
GRANT ALL ON SCHEMA public TO postgres;
GRANT ALL ON SCHEMA public TO PUBLIC;


-- Completed on 2016-09-08 16:28:58

--
-- PostgreSQL database dump complete
--

