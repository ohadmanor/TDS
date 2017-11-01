CREATE TABLE IF NOT EXISTS cultural_data(
	guid varchar(50)  NOT NULL,	
	age int NOT NULL,
	gender varchar(10)  NOT NULL,
	country varchar(50)  NOT NULL,
	formation_type varchar(50) NULL,
	formation_males int NULL,
	formation_females int NULL,
	personal_space float NULL,
	social_space float NULL,
	public_space float NULL,
	avoidance_side_left_probability float NULL,
	speed float NULL,
	PRIMARY KEY(guid)
);

CREATE TABLE IF NOT EXISTS cultural_gender_bias(
	guid varchar(50) NOT NULL,
	country varchar(50) NOT NULL,
	bias float NOT NULL,
	PRIMARY_KEY(guid)
);