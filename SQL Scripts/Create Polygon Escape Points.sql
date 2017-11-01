-- Table: polygon_openings_escape_points

-- DROP TABLE polygon_openings_escape_points;

CREATE TABLE IF NOT EXISTS polygon_openings_escape_points
(
  polygon_guid character varying(50) NOT NULL,
  polygon_edge_num integer NOT NULL,
  route_x numeric NOT NULL,
  route_y numeric NOT NULL
)
WITH (
  OIDS=FALSE
);
ALTER TABLE polygon_openings_escape_points
  OWNER TO postgres;
