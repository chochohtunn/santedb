﻿/** 
 * <feature scope="SanteDB.Persistence.Data.ADO" id="20170725-01" name="Update:20170725-01" applyRange="0.2.0.0-0.9.0.1"  invariantName="npgsql">
 *	<summary>Update: Switches high volume tables to integer keys rather than UUIDs</summary>
 *	<remarks>This update will add numeric keys to tables and updates linkages between these
 *	for data which is not submitted by or exposed to external parties.</remarks>
 *	<isInstalled>select ck_patch('20170725-01')</isInstalled>
 * </feature>
 */

 BEGIN TRANSACTION ;

 CREATE SEQUENCE ENT_ADDR_CMP_VAL_SEQ START WITH 1 INCREMENT BY 1;

 -- CREATE SEQUENCE TABLE ON ADDRESS VALUES
 ALTER TABLE ENT_ADDR_CMP_VAL_TBL ADD VAL_SEQ_ID NUMERIC(10,0) NOT NULL DEFAULT nextval('ENT_ADDR_CMP_VAL_SEQ');
 CREATE UNIQUE INDEX ENT_ADDR_CMP_VAL_SEQ_ID ON ENT_ADDR_CMP_VAL_TBL(VAL_SEQ_ID);

 -- ALTER ENT_ADDR_CMP_TBL TO USE SEQ
 ALTER TABLE ENT_ADDR_CMP_TBL ADD VAL_SEQ_ID NUMERIC(10,0);
 UPDATE ENT_ADDR_CMP_TBL SET VAL_SEQ_ID = (SELECT VAL_SEQ_ID FROM ENT_ADDR_CMP_VAL_TBL WHERE ENT_ADDR_CMP_VAL_TBL.VAL_ID = ENT_ADDR_CMP_TBL.VAL_ID);
 ALTER TABLE ENT_ADDR_CMP_TBL ALTER VAL_SEQ_ID SET NOT NULL;
 
 DROP INDEX ENT_ADDR_CMP_VAL_ID_IDX;
 CREATE INDEX ENT_ADDR_CMP_VAL_IDX ON ENT_ADDR_CMP_TBL(VAL_SEQ_ID);
 
 ALTER TABLE ENT_ADDR_CMP_TBL DROP VAL_ID CASCADE;
 ALTER TABLE ENT_ADDR_CMP_TBL ADD CONSTRAINT FK_ENT_ADDR_CMP_VAL_TBL FOREIGN KEY (VAL_SEQ_ID) REFERENCES ENT_ADDR_CMP_VAL_TBL(VAL_SEQ_ID);

 -- DO THE SAME FOR NAMES
 CREATE SEQUENCE PHON_VAL_SEQ START WITH 1 INCREMENT BY 1;

 ALTER TABLE PHON_VAL_TBL ADD VAL_SEQ_ID NUMERIC(10,0) NOT NULL DEFAULT nextval('PHON_VAL_SEQ');
 CREATE UNIQUE INDEX ENT_NAME_CMP_VAL_SEQ_ID ON PHON_VAL_TBL(VAL_SEQ_ID);

 -- ALTER ENT_NAME_CMP_TBL TO USE SEQ
 ALTER TABLE ENT_NAME_CMP_TBL ADD VAL_SEQ_ID NUMERIC(10,0);
 UPDATE ENT_NAME_CMP_TBL SET VAL_SEQ_ID = (SELECT VAL_SEQ_ID FROM PHON_VAL_TBL WHERE PHON_VAL_TBL.VAL_ID = ENT_NAME_CMP_TBL.VAL_ID);
 ALTER TABLE ENT_NAME_CMP_TBL ALTER VAL_SEQ_ID SET NOT NULL;
 
 DROP INDEX ENT_NAME_CMP_PHON_VAL_ID_IDX;
 CREATE INDEX ENT_NAME_CMP_PHON_VAL_IDX ON ENT_NAME_CMP_TBL(VAL_SEQ_ID);
 
 ALTER TABLE ENT_NAME_CMP_TBL DROP VAL_ID CASCADE;
  
 ALTER TABLE ENT_NAME_CMP_TBL ADD CONSTRAINT FK_PHON_VAL_TBL FOREIGN KEY (VAL_SEQ_ID) REFERENCES PHON_VAL_TBL(VAL_SEQ_ID);

 ALTER TABLE ENT_NAME_CMP_TBL RENAME VAL_SEQ TO CMP_SEQ;

 CREATE INDEX SEC_USR_ROL_ASSOC_USR_IDX ON SEC_USR_ROL_ASSOC_TBL(USR_ID);
 CREATE INDEX SEC_ROL_POL_ASSOC_POL_IDX ON SEC_ROL_POL_ASSOC_TBL(POL_ID);

 /*
 CREATE SEQUENCE ENT_ADDR_SEQ START WITH 1 INCREMENT BY 1;
 ALTER TABLE ENT_ADDR_TBL ADD ADDR_SEQ_ID NUMERIC(10,0) NOT NULL DEFAULT NEXTVAL('ENT_ADDR_SEQ');
 ALTER TABLE ENT_ADDR_CMP_TBL ADD ADDR_SEQ_ID NUMERIC(10,0);
 UPDATE ENT_ADDR_CMP_TBL SET ADDR_SEQ_ID = (SELECT ADDR_SEQ_ID FROM ENT_ADDR_TBL WHERE ENT_ADDR_TBL.ADDR_ID = ENT_ADDR_CMP_TBL.ADDR_ID);
 ALTER TABLE ENT_ADDR_CMP_TBL ALTER ADDR_SEQ_ID SET NOT NULL;
 CREATE UNIQUE INDEX ENT_ADDR_ADDR_SEQ_ID ON ENT_ADDR_TBL(ADDR_SEQ_ID);
 CREATE INDEX ENT_ADDR_CMP_ADDR_SEQ_IDX ON ENT_ADDR_CMP_TBL(ADDR_SEQ_ID);
 ALTER TABLE ENT_ADDR_CMP_TBL ADD FOREIGN KEY (ADDR_SEQ_ID) REFERENCES ENT_ADDR_TBL(ADDR_SEQ_ID);
 */

 -- GET THE SCHEMA VERSION
CREATE OR REPLACE FUNCTION GET_SCH_VRSN() RETURNS VARCHAR(10) AS
$$
BEGIN
	RETURN '0.9.0.2';
END;
$$ LANGUAGE plpgsql;

SELECT REG_PATCH('20170725-01');

COMMIT;