-- Table: polygon_openings

-- DROP TABLE polygon_openings;

CREATE TABLE IF NOT EXISTS polygon_openings
(
  opening_guid character varying(50) NOT NULL,
  polygon_guid character varying(50) NOT NULL,
  polygon_edge_num integer NOT NULL,
  position_x numeric NOT NULL,
  position_y numeric NOT NULL,
  opening_size_meters numeric NOT NULL,
  CONSTRAINT polygon_openings_pkey PRIMARY KEY (opening_guid)
)
WITH (
  OIDS=FALSE
);
ALTER TABLE polygon_openings
  OWNER TO postgres;
