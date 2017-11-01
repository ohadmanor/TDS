-- Table: barriers

-- DROP TABLE barriers;

CREATE TABLE IF NOT EXISTS barriers
(
  guid character varying(50) NOT NULL,
  x double precision,
  y double precision,
  angle double precision,
  CONSTRAINT barriers_pkey PRIMARY KEY (guid)
)
WITH (
  OIDS=FALSE
);
ALTER TABLE barriers
  OWNER TO postgres;