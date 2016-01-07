CREATE DATABASE "TDS"
  WITH OWNER = postgres
       ENCODING = 'UTF8'
       TABLESPACE = pg_default
       LC_COLLATE = 'Hebrew_Israel.1255'
       LC_CTYPE = 'Hebrew_Israel.1255'
       CONNECTION LIMIT = -1;

ALTER DATABASE "TDS"
  SET search_path = "$user", public, topology, tiger;