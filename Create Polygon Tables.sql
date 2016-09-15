CREATE TABLE IF NOT EXISTS Polygons(
	polygon_guid varchar(50)  NOT NULL,
	polygon_name varchar(255)  NOT NULL,
	polygon_name_X float NULL,
    polygon_name_Y float NULL,
    polygon_fill_color varchar(50) NULL,
    polygon_fill_transparency float NULL,
    polygon_border_color varchar(50) NULL,
    polygon_border_width int NULL,
    polygon_text_color varchar(50) NULL,
    point_icon varchar(500) NULL,
    polygon_geometry_type int NULL,
    polygon_layer_guid varchar(50) NULL,
    polygon_name_fontsize int NULL,	
	 PRIMARY KEY(polygon_guid)
);
CREATE TABLE IF NOT EXISTS Polygon_Points(
	polygon_guid varchar(50)  NOT NULL,
	point_num int  NOT NULL,
	pointX float NULL,
	pointY float NULL,
	 PRIMARY KEY(polygon_guid,point_num)
);
insert into polygons (polygon_guid,polygon_name) values ('5463C3C4-D904-4F96-A268-1E8A29885739','Polygon1'); 

insert into polygon_points (polygon_guid,point_num,pointx,pointy) values ('5463C3C4-D904-4F96-A268-1E8A29885739',0,34.848807,32.099571);
insert into polygon_points (polygon_guid,point_num,pointx,pointy) values ('5463C3C4-D904-4F96-A268-1E8A29885739',1,34.849351,32.099571);
insert into polygon_points (polygon_guid,point_num,pointx,pointy) values ('5463C3C4-D904-4F96-A268-1E8A29885739',2,34.849351,32.099028);
insert into polygon_points (polygon_guid,point_num,pointx,pointy) values ('5463C3C4-D904-4F96-A268-1E8A29885739',3,34.850344,32.099028);
insert into polygon_points (polygon_guid,point_num,pointx,pointy) values ('5463C3C4-D904-4F96-A268-1E8A29885739',4,34.850344,32.098317);
insert into polygon_points (polygon_guid,point_num,pointx,pointy) values ('5463C3C4-D904-4F96-A268-1E8A29885739',5,34.848807,32.098317);