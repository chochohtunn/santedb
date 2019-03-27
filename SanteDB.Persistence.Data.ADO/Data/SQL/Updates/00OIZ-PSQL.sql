﻿/** 
 * <feature scope="SanteDB.Persistence.Data.ADO" id="00-OPENIZ" name="Upgrade from OpenIZ 1.1.0.0" applyRange="1.1.0.0-1.1.0.0" invariantName="npgsql">
 *	<summary>Upgrades an OpenIZ 1.1.0.0 database SanteDB</summary>
 *	<remarks>This update will perform the necessary steps to upgrade an existing OpenIZ database to SanteDB</remarks>
 *  <isInstalled>SELECT COUNT(*) FROM SEC_PROV_TBL;</isInstalled>
 * </feature>
 */
 
 
-- SECURITY PROVENANCE TABLE
-- THIS TABLE IS USED TO RECORD THE PROVENANCE OF AN OBJECT AT THE TIME OF CREATION / UPDATION / ETC.
CREATE TABLE SEC_PROV_TBL (
	PROV_ID UUID NOT NULL DEFAULT uuid_generate_v1(), -- THE ID OF THE PROVENANCE EVENT
	USR_ID UUID, -- THE USER IDENTITY OF THE EVENT
	DEV_ID UUID, -- THE DEVICE ID OF THE EVENT
	APP_ID UUID NOT NULL, -- THE APPLICATION THE USER OR DEVICE WAS USING
	SES_ID UUID, -- THE SESSION OF THE EVENT IF APPLICABLE - NB THE SESSION MECHANISM MAY NOT BE IN THIS DATABASE SO THERE IS NO FK TO THE SESSION IDENTIFIER
	EST_UTC TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP, -- THE TIMESTAMP WHEN THE TRANSACTION BEGAN ON THE SERVER (REGARDLESS OF WHEN THE DEVICE SAID THE CREATION TIME WAS)
	EXT_ID UUID,
	EXT_TYP CHAR(1) CHECK (EXT_TYP IN ('U','P')),
	CONSTRAINT PK_SEC_PROV_TBL PRIMARY KEY (PROV_ID),
	CONSTRAINT CK_SEC_PROV_ORGN CHECK (USR_ID IS NOT NULL OR DEV_ID IS NOT NULL OR SES_ID IS NOT NULL)
);

-- 
INSERT INTO SEC_PROV_TBL (PROV_ID, USR_ID, APP_ID) VALUES ('fadca076-3690-4a6e-af9e-f1cd68e8c7e8', 'FADCA076-3690-4A6E-AF9E-F1CD68E8C7E8', '4c5b9f8d-49f4-4101-9662-4270895224b2');

ALTER TABLE SEC_USR_TBL RENAME CRT_USR_ID TO CRT_PROV_ID;
ALTER TABLE SEC_USR_TBL RENAME UPD_USR_ID TO UPD_PROV_ID;
ALTER TABLE SEC_USR_TBL RENAME OBSLT_USR_ID TO OBSLT_PROV_ID;

-- INSERT PROVENANCES
INSERT INTO SEC_PROV_TBL SELECT USR_ID, USR_ID, NULL, '4c5b9f8d-49f4-4101-9662-4270895224b2', NULL, CURRENT_TIMESTAMP, NULL, NULL FROM SEC_USR_TBL WHERE USR_ID <> 'fadca076-3690-4a6e-af9e-f1cd68e8c7e8';

-- UPDATE CONSTRAINTS
ALTER TABLE SEC_USR_TBL DROP CONSTRAINT fk_sec_usr_crt_usr_id;
ALTER TABLE SEC_USR_TBL DROP CONSTRAINT fk_sec_usr_obslt_usr_id;
ALTER TABLE SEC_USR_TBL DROP CONSTRAINT fk_sec_usr_upd_usr_id;
ALTER TABLE SEC_USR_TBL DROP CONSTRAINT ck_sec_usr_obslt_usr;
ALTER TABLE SEC_USR_TBL DROP CONSTRAINT ck_sec_usr_upd_usr;
ALTER TABLE SEC_USR_TBL ADD CONSTRAINT FK_SEC_USR_OBSLT_PROV_ID FOREIGN KEY(OBSLT_PROV_ID) REFERENCES SEC_PROV_TBL(PROV_ID);
ALTER TABLE SEC_USR_TBL ADD CONSTRAINT FK_SEC_USR_UPD_PROV_ID FOREIGN KEY (UPD_PROV_ID) REFERENCES SEC_PROV_TBL(PROV_ID);
ALTER TABLE SEC_USR_TBL ADD CONSTRAINT FK_SEC_USR_CRT_PROV_ID FOREIGN KEY (CRT_PROV_ID) REFERENCES SEC_PROV_TBL(PROV_ID);
ALTER TABLE SEC_USR_TBL ADD CONSTRAINT CK_SEC_USR_OBSLT_USR CHECK (OBSLT_PROV_ID IS NOT NULL AND OBSLT_UTC IS NOT NULL OR OBSLT_PROV_ID IS NULL AND OBSLT_UTC IS NULL);
ALTER TABLE SEC_USR_TBL ADD CONSTRAINT CK_SEC_USR_UPD_USR CHECK (UPD_PROV_ID IS NOT NULL AND UPD_UTC IS NOT NULL OR UPD_PROV_ID IS NULL AND UPD_UTC IS NULL);

--#!

ALTER TABLE SEC_ROL_TBL RENAME CRT_USR_ID TO CRT_PROV_ID;
ALTER TABLE SEC_ROL_TBL RENAME UPD_USR_ID TO UPD_PROV_ID;
ALTER TABLE SEC_ROL_TBL RENAME OBSLT_USR_ID TO OBSLT_PROV_ID;
ALTER TABLE SEC_ROL_TBL DROP CONSTRAINT fk_sec_rol_crt_usr_id;
ALTER TABLE SEC_ROL_TBL DROP CONSTRAINT fk_sec_rol_obslt_usr_id;
ALTER TABLE SEC_ROL_TBL DROP CONSTRAINT fk_sec_rol_upd_usr_id;
ALTER TABLE SEC_ROL_TBL ADD CONSTRAINT FK_SEC_ROL_CRT_PROV_ID FOREIGN KEY (CRT_PROV_ID) REFERENCES SEC_PROV_TBL(PROV_ID);
ALTER TABLE SEC_ROL_TBL ADD CONSTRAINT FK_SEC_ROL_OBSLT_PROV_ID FOREIGN KEY (OBSLT_PROV_ID) REFERENCES SEC_PROV_TBL(PROV_ID);
ALTER TABLE SEC_ROL_TBL ADD CONSTRAINT FK_SEC_ROL_UPD_PROV_ID FOREIGN KEY (UPD_PROV_ID) REFERENCES SEC_PROV_TBL(PROV_ID);

--#!

ALTER TABLE SEC_POL_TBL RENAME CRT_USR_ID TO CRT_PROV_ID;
ALTER TABLE SEC_POL_TBL RENAME OBSLT_USR_ID TO OBSLT_PROV_ID;
ALTER TABLE SEC_POL_TBL DROP CONSTRAINT FK_SEC_POL_CRT_UTC;
ALTER TABLE SEC_POL_TBL DROP CONSTRAINT FK_SEC_POL_OBSLT_UTC;
ALTER TABLE SEC_POL_TBL ADD CONSTRAINT FK_SEC_POL_CRT_PROV_ID FOREIGN KEY (CRT_PROV_ID) REFERENCES SEC_PROV_TBL(PROV_ID);
ALTER TABLE SEC_POL_TBL ADD CONSTRAINT FK_SEC_POL_OBSLT_PROV_ID FOREIGN KEY (OBSLT_PROV_ID) REFERENCES SEC_PROV_TBL(PROV_ID);


INSERT INTO SEC_POL_TBL (POL_ID, OID, POL_NAME, CRT_PROV_ID, IS_ELEV) VALUES ('36f1ed35-552e-421a-8f59-629561ab9eb6', '1.3.6.1.4.1.33349.3.1.5.9.3', 'Restricted Information', 'fadca076-3690-4a6e-af9e-f1cd68e8c7e8', TRUE);

INSERT INTO SEC_POL_TBL (POL_ID, OID, POL_NAME, CRT_PROV_ID) VALUES (('baa124aa-224d-4859-81b3-c1eb2750067e'), '1.3.6.1.4.1.33349.3.1.5.9.2.4.1.0', 'Write Materials', ('fadca076-3690-4a6e-af9e-f1cd68e8c7e8'));
INSERT INTO SEC_POL_TBL (POL_ID, OID, POL_NAME, CRT_PROV_ID) VALUES (('baa125aa-224d-4859-81b3-c1eb2750067e'), '1.3.6.1.4.1.33349.3.1.5.9.2.4.1.1', 'Delete Materials', ('fadca076-3690-4a6e-af9e-f1cd68e8c7e8'));
INSERT INTO SEC_POL_TBL (POL_ID, OID, POL_NAME, CRT_PROV_ID) VALUES (('baa126aa-224d-4859-81b3-c1eb2750067e'), '1.3.6.1.4.1.33349.3.1.5.9.2.4.0.1.2', 'Read Materials', ('fadca076-3690-4a6e-af9e-f1cd68e8c7e8'));
INSERT INTO SEC_POL_TBL (POL_ID, OID, POL_NAME, CRT_PROV_ID) VALUES (('baa127aa-224d-4859-81b3-c1eb2750067e'), '1.3.6.1.4.1.33349.3.1.5.9.2.4.0.1.3', 'Query Materials', ('fadca076-3690-4a6e-af9e-f1cd68e8c7e8'));
INSERT INTO SEC_POL_TBL (POL_ID, OID, POL_NAME, CRT_PROV_ID) VALUES (('baa224aa-224d-4859-81b3-c1eb2750067e'), '1.3.6.1.4.1.33349.3.1.5.9.2.4.2.0', 'Write Places & Orgs', ('fadca076-3690-4a6e-af9e-f1cd68e8c7e8'));
INSERT INTO SEC_POL_TBL (POL_ID, OID, POL_NAME, CRT_PROV_ID) VALUES (('baa225aa-224d-4859-81b3-c1eb2750067e'), '1.3.6.1.4.1.33349.3.1.5.9.2.4.2.1', 'Delete Places & Orgs', ('fadca076-3690-4a6e-af9e-f1cd68e8c7e8'));
INSERT INTO SEC_POL_TBL (POL_ID, OID, POL_NAME, CRT_PROV_ID) VALUES (('baa226aa-224d-4859-81b3-c1eb2750067e'), '1.3.6.1.4.1.33349.3.1.5.9.2.4.0.2.2', 'Read Places & Orgs', ('fadca076-3690-4a6e-af9e-f1cd68e8c7e8'));
INSERT INTO SEC_POL_TBL (POL_ID, OID, POL_NAME, CRT_PROV_ID) VALUES (('baa227aa-224d-4859-81b3-c1eb2750067e'), '1.3.6.1.4.1.33349.3.1.5.9.2.4.0.2.3', 'Query Places & Orgs', ('fadca076-3690-4a6e-af9e-f1cd68e8c7e8'));

--#!

ALTER TABLE SEC_DEV_TBL RENAME CRT_USR_ID TO CRT_PROV_ID;
ALTER TABLE SEC_DEV_TBL RENAME OBSLT_USR_ID TO OBSLT_PROV_ID;
ALTER TABLE SEC_DEV_TBL DROP CONSTRAINT fk_sec_dev_crt_usr_id;
ALTER TABLE SEC_DEV_TBL DROP CONSTRAINT fk_sec_dev_obslt_usr_id;
ALTER TABLE SEC_DEV_TBL ADD CONSTRAINT FK_SEC_DEV_CRT_PROV_ID FOREIGN KEY (CRT_PROV_ID) REFERENCES SEC_PROV_TBL(PROV_ID);
ALTER TABLE SEC_DEV_TBL ADD CONSTRAINT FK_SEC_DEV_OBSLT_PROV_ID FOREIGN KEY (OBSLT_PROV_ID) REFERENCES SEC_PROV_TBL(PROV_ID);

--#!

ALTER TABLE SEC_APP_TBL RENAME CRT_USR_ID TO CRT_PROV_ID;
ALTER TABLE SEC_APP_TBL RENAME OBSLT_USR_ID TO OBSLT_PROV_ID;
ALTER TABLE SEC_APP_TBL DROP CONSTRAINT fk_sec_app_crt_usr_id;
ALTER TABLE SEC_APP_TBL DROP CONSTRAINT fk_sec_app_obslt_usr_id;
ALTER TABLE SEC_APP_TBL ADD CONSTRAINT FK_SEC_APP_CRT_PROV_ID FOREIGN KEY (CRT_PROV_ID) REFERENCES SEC_PROV_TBL(PROV_ID);
ALTER TABLE SEC_APP_TBL ADD CONSTRAINT FK_SEC_APP_OBSLT_PROV_ID FOREIGN KEY (OBSLT_PROV_ID) REFERENCES SEC_PROV_TBL(PROV_ID);
INSERT INTO SEC_APP_TBL (APP_ID, APP_PUB_ID, APP_SCRT, CRT_PROV_ID) VALUES ('4c5b9f8d-49f4-4101-9662-4270895224b2', 'SYSTEM', 'XXXXX', 'fadca076-3690-4a6e-af9e-f1cd68e8c7e8');

--#!

ALTER TABLE SEC_PROV_TBL ADD CONSTRAINT FK_SEC_PROV_APP FOREIGN KEY (APP_ID) REFERENCES SEC_APP_TBL(APP_ID);
ALTER TABLE SEC_PROV_TBL ADD CONSTRAINT FK_SEC_PROV_USR FOREIGN KEY (USR_ID) REFERENCES SEC_USR_TBL(USR_ID);
ALTER TABLE SEC_PROV_TBL ADD CONSTRAINT FK_SEC_PROV_DEV FOREIGN KEY (DEV_ID) REFERENCES SEC_DEV_TBL(DEV_ID);

--#!

ALTER TABLE PHON_ALG_TBL RENAME CRT_USR_ID TO CRT_PROV_ID;
ALTER TABLE PHON_ALG_TBL RENAME UPD_USR_ID TO UPD_PROV_ID;
ALTER TABLE PHON_ALG_TBL RENAME OBSLT_USR_ID TO OBSLT_PROV_ID;
ALTER TABLE PHON_ALG_TBL DROP CONSTRAINT fk_phon_alg_crt_usr_id;
ALTER TABLE PHON_ALG_TBL DROP CONSTRAINT fk_phon_alg_obslt_usr_id;
ALTER TABLE PHON_ALG_TBL DROP CONSTRAINT fk_phon_alg_upd_usr_id;
ALTER TABLE PHON_ALG_TBL ADD CONSTRAINT FK_PHON_ALG_CRT_PROV_ID FOREIGN KEY (CRT_PROV_ID) REFERENCES SEC_PROV_TBL(PROV_ID);
ALTER TABLE PHON_ALG_TBL ADD CONSTRAINT FK_PHON_ALG_UPD_PROV_ID FOREIGN KEY (UPD_PROV_ID) REFERENCES SEC_PROV_TBL(PROV_ID);
ALTER TABLE PHON_ALG_TBL ADD CONSTRAINT FK_PHON_ALG_OBSLT_PROV_ID FOREIGN KEY (OBSLT_PROV_ID) REFERENCES SEC_PROV_TBL(PROV_ID);

--#!

ALTER TABLE CD_CLS_TBL RENAME CRT_USR_ID TO CRT_PROV_ID;
ALTER TABLE CD_CLS_TBL RENAME UPD_USR_ID TO UPD_PROV_ID;
ALTER TABLE CD_CLS_TBL RENAME OBSLT_USR_ID TO OBSLT_PROV_ID;
ALTER TABLE CD_CLS_TBL DROP CONSTRAINT fk_cd_cls_crt_usr_id;
ALTER TABLE CD_CLS_TBL DROP CONSTRAINT fk_cd_cls_obslt_usr_id;
ALTER TABLE CD_CLS_TBL DROP CONSTRAINT fk_cd_cls_upd_usr_id;
ALTER TABLE CD_CLS_TBL ADD CONSTRAINT FK_CD_CLS_CRT_PROV_ID FOREIGN KEY (CRT_PROV_ID) REFERENCES SEC_PROV_TBL(PROV_ID);
ALTER TABLE CD_CLS_TBL ADD CONSTRAINT FK_CD_CLS_UPD_PROV_ID FOREIGN KEY (UPD_PROV_ID) REFERENCES SEC_PROV_TBL(PROV_ID);
ALTER TABLE CD_CLS_TBL ADD CONSTRAINT FK_CD_CLS_OBSLT_PROV_ID FOREIGN KEY (OBSLT_PROV_ID) REFERENCES SEC_PROV_TBL(PROV_ID);

--#!

ALTER TABLE CD_SET_TBL RENAME CRT_USR_ID TO CRT_PROV_ID;
ALTER TABLE CD_SET_TBL RENAME UPD_USR_ID TO UPD_PROV_ID;
ALTER TABLE CD_SET_TBL RENAME OBSLT_USR_ID TO OBSLT_PROV_ID;
ALTER TABLE CD_SET_TBL DROP CONSTRAINT fk_cd_set_crt_usr_id;
ALTER TABLE CD_SET_TBL DROP CONSTRAINT fk_cd_set_obslt_usr_id;
ALTER TABLE CD_SET_TBL DROP CONSTRAINT fk_cd_set_upd_usr_id;
ALTER TABLE CD_SET_TBL ADD CONSTRAINT FK_CD_SET_CRT_PROV_ID FOREIGN KEY (CRT_PROV_ID) REFERENCES SEC_PROV_TBL(PROV_ID);
ALTER TABLE CD_SET_TBL ADD CONSTRAINT FK_CD_SET_UPD_PROV_ID FOREIGN KEY (UPD_PROV_ID) REFERENCES SEC_PROV_TBL(PROV_ID);
ALTER TABLE CD_SET_TBL ADD CONSTRAINT FK_CD_SET_OBSLT_PROV_ID FOREIGN KEY (OBSLT_PROV_ID) REFERENCES SEC_PROV_TBL(PROV_ID);

--#!

ALTER TABLE CD_VRSN_TBL RENAME CRT_USR_ID TO CRT_PROV_ID;
ALTER TABLE CD_VRSN_TBL RENAME OBSLT_USR_ID TO OBSLT_PROV_ID;
ALTER TABLE CD_VRSN_TBL DROP CONSTRAINT fk_cd_vrsn_crt_usr_id;
ALTER TABLE CD_VRSN_TBL DROP CONSTRAINT fk_cd_vrsn_obslt_usr_id;
ALTER TABLE CD_VRSN_TBL ADD CONSTRAINT FK_CD_VRSN_CRT_PROV_ID FOREIGN KEY (CRT_PROV_ID) REFERENCES SEC_PROV_TBL(PROV_ID);
ALTER TABLE CD_VRSN_TBL ADD CONSTRAINT FK_CD_VRSN_OBSLT_PROV_ID FOREIGN KEY (OBSLT_PROV_ID) REFERENCES SEC_PROV_TBL(PROV_ID);

--#!

ALTER TABLE CD_SYS_TBL RENAME CRT_USR_ID TO CRT_PROV_ID;
ALTER TABLE CD_SYS_TBL RENAME UPD_USR_ID TO UPD_PROV_ID;
ALTER TABLE CD_SYS_TBL RENAME OBSLT_USR_ID TO OBSLT_PROV_ID;
ALTER TABLE CD_SYS_TBL DROP CONSTRAINT fk_cd_sys_crt_usr_id;
ALTER TABLE CD_SYS_TBL DROP CONSTRAINT fk_cd_sys_obslt_usr_id;
ALTER TABLE CD_SYS_TBL DROP CONSTRAINT fk_cd_sys_upd_usr_id;
ALTER TABLE CD_SYS_TBL ADD CONSTRAINT FK_CD_SYS_CRT_PROV_ID FOREIGN KEY (CRT_PROV_ID) REFERENCES SEC_PROV_TBL(PROV_ID);
ALTER TABLE CD_SYS_TBL ADD CONSTRAINT FK_CD_SYS_UPD_PROV_ID FOREIGN KEY (UPD_PROV_ID) REFERENCES SEC_PROV_TBL(PROV_ID);
ALTER TABLE CD_SYS_TBL ADD CONSTRAINT FK_CD_SYS_OBSLT_PROV_ID FOREIGN KEy (OBSLT_PROV_ID) REFERENCES SEC_PROV_TBL(PROV_ID);

--#!

ALTER TABLE CD_REL_TYP_CDTBL RENAME CRT_USR_ID TO CRT_PROV_ID;
ALTER TABLE CD_REL_TYP_CDTBL RENAME UPD_USR_ID TO UPD_PROV_ID;
ALTER TABLE CD_REL_TYP_CDTBL RENAME OBSLT_USR_ID TO OBSLT_PROV_ID;
ALTER TABLE CD_REL_TYP_CDTBL DROP CONSTRAINT fk_cd_rel_crt_usr_id;
ALTER TABLE CD_REL_TYP_CDTBL DROP CONSTRAINT fk_cd_rel_obslt_usr_id;
ALTER TABLE CD_REL_TYP_CDTBL DROP CONSTRAINT fk_cd_rel_upd_usr_id;
ALTER TABLE CD_REL_TYP_CDTBL ADD CONSTRAINT FK_CD_REL_CRT_PROV_ID FOREIGN KEY (CRT_PROV_ID) REFERENCES SEC_PROV_TBL(PROV_ID);
ALTER TABLE CD_REL_TYP_CDTBL ADD CONSTRAINT FK_CD_REL_UPD_PROV_ID FOREIGN KEY (UPD_PROV_ID) REFERENCES SEC_PROV_TBL(PROV_ID);
ALTER TABLE CD_REL_TYP_CDTBL ADD CONSTRAINT FK_CD_REL_OBSLT_PROV_ID FOREIGN KEy (OBSLT_PROV_ID) REFERENCES SEC_PROV_TBL(PROV_ID);

--#!

ALTER TABLE REF_TERM_TBL RENAME CRT_USR_ID TO CRT_PROV_ID;
ALTER TABLE REF_TERM_TBL RENAME UPD_USR_ID TO UPD_PROV_ID;
ALTER TABLE REF_TERM_TBL RENAME OBSLT_USR_ID TO OBSLT_PROV_ID;
ALTER TABLE REF_TERM_TBL DROP CONSTRAINT fk_ref_term_crt_usr_id;
ALTER TABLE REF_TERM_TBL DROP CONSTRAINT fk_ref_term_obslt_usr_id;
ALTER TABLE REF_TERM_TBL DROP CONSTRAINT fk_ref_term_upd_usr_id;
ALTER TABLE REF_TERM_TBL ADD CONSTRAINT FK_REF_TERM_CRT_PROV_ID FOREIGN KEY (CRT_PROV_ID) REFERENCES SEC_PROV_TBL(PROV_ID);
ALTER TABLE REF_TERM_TBL ADD CONSTRAINT FK_REF_TERM_UPD_PROV_ID FOREIGN KEY (UPD_PROV_ID) REFERENCES SEC_PROV_TBL(PROV_ID);
ALTER TABLE REF_TERM_TBL ADD CONSTRAINT FK_REF_TERM_OBSLT_PROV_ID FOREIGN KEY (OBSLT_PROV_ID) REFERENCES SEC_PROV_TBL(PROV_ID);

--#!

ALTER TABLE REF_TERM_NAME_TBL RENAME CRT_USR_ID TO CRT_PROV_ID;
ALTER TABLE REF_TERM_NAME_TBL RENAME OBSLT_USR_ID TO OBSLT_PROV_ID;
ALTER TABLE REF_TERM_NAME_TBL DROP CONSTRAINT fk_ref_term_name_crt_usr_id;
ALTER TABLE REF_TERM_NAME_TBL DROP CONSTRAINT fk_ref_term_name_obslt_usr_id;
ALTER TABLE REF_TERM_NAME_TBL ADD CONSTRAINT FK_REF_TERM_NAME_CRT_PROV_ID FOREIGN KEY (CRT_PROV_ID) REFERENCES SEC_PROV_TBL(PROV_ID);
ALTER TABLE REF_TERM_NAME_TBL ADD CONSTRAINT FK_REF_TERM_NAME_OBSLT_PROV_ID FOREIGN KEY (OBSLT_PROV_ID) REFERENCES SEC_PROV_TBL(PROV_ID);

--#!

ALTER TABLE EXT_TYP_TBL RENAME CRT_USR_ID TO CRT_PROV_ID;
ALTER TABLE EXT_TYP_TBL RENAME UPD_USR_ID TO UPD_PROV_ID;
ALTER TABLE EXT_TYP_TBL RENAME OBSLT_USR_ID TO OBSLT_PROV_ID;
ALTER TABLE EXT_TYP_TBL DROP CONSTRAINT fk_ext_typ_crt_usr_id;
ALTER TABLE EXT_TYP_TBL DROP CONSTRAINT fk_ext_typ_obslt_usr_id;
ALTER TABLE EXT_TYP_TBL DROP CONSTRAINT fk_ext_typ_upd_usr_id;
ALTER TABLE EXT_TYP_TBL ADD CONSTRAINT FK_EXT_TYP_CRT_PROV_ID FOREIGN KEY (CRT_PROV_ID) REFERENCES SEC_PROV_TBL(PROV_ID);
ALTER TABLE EXT_TYP_TBL ADD CONSTRAINT FK_EXT_TYP_UPD_PROV_ID FOREIGN KEY (UPD_PROV_ID) REFERENCES SEC_PROV_TBL(PROV_ID);
ALTER TABLE EXT_TYP_TBL ADD CONSTRAINT FK_EXT_TYP_OBSLT_PROV_ID FOREIGN KEY (OBSLT_PROV_ID) REFERENCES SEC_PROV_TBL(PROV_ID);

--#!

ALTER TABLE ASGN_AUT_TBL RENAME CRT_USR_ID TO CRT_PROV_ID;
ALTER TABLE ASGN_AUT_TBL RENAME OBSLT_USR_ID TO OBSLT_PROV_ID;
ALTER TABLE ASGN_AUT_TBL DROP CONSTRAINT fk_asgn_aut_crt_usr_id;
ALTER TABLE ASGN_AUT_TBL DROP CONSTRAINT fk_asgn_aut_obslt_usr_id;
ALTER TABLE ASGN_AUT_TBL ADD CONSTRAINT FK_ASGN_AUT_CRT_PROV_ID FOREIGN KEY (CRT_PROV_ID) REFERENCES SEC_PROV_TBL(PROV_ID);
ALTER TABLE ASGN_AUT_TBL ADD CONSTRAINT FK_ASGN_AUT_OBSLT_PROV_ID FOREIGN KEY (OBSLT_PROV_ID) REFERENCES SEC_PROV_TBL(PROV_ID);

--#!

ALTER TABLE ID_TYP_TBL RENAME CRT_USR_ID TO CRT_PROV_ID;
ALTER TABLE ID_TYP_TBL RENAME OBSLT_USR_ID TO OBSLT_PROV_ID;
ALTER TABLE ID_TYP_TBL DROP CONSTRAINT fk_id_typ_crt_usr_id;
ALTER TABLE ID_TYP_TBL DROP CONSTRAINT fk_id_typ_obslt_usr_id;
ALTER TABLE ID_TYP_TBL ADD CONSTRAINT FK_ID_TYP_CRT_PROV_ID FOREIGN KEY (CRT_PROV_ID) REFERENCES SEC_PROV_TBL(PROV_ID);
ALTER TABLE ID_TYP_TBL ADD CONSTRAINT FK_ID_TYP_OBSLT_PROV_ID FOREIGN KEY (OBSLT_PROV_ID) REFERENCES SEC_PROV_TBL(PROV_ID);

--#!

ALTER TABLE TPL_DEF_TBL RENAME CRT_USR_ID TO CRT_PROV_ID;
ALTER TABLE TPL_DEF_TBL RENAME UPD_USR_ID TO UPD_PROV_ID;
ALTER TABLE TPL_DEF_TBL RENAME OBSLT_USR_ID TO OBSLT_PROV_ID;
ALTER TABLE TPL_DEF_TBL DROP CONSTRAINT fk_tpl_def_crt_usr_id;
ALTER TABLE TPL_DEF_TBL DROP CONSTRAINT fk_tpl_def_obslt_usr_id;
ALTER TABLE TPL_DEF_TBL DROP CONSTRAINT fk_tpl_def_upd_usr_id;
ALTER TABLE TPL_DEF_TBL ADD CONSTRAINT FK_TPL_DEF_CRT_PROV_ID FOREIGN KEY (CRT_PROV_ID) REFERENCES SEC_PROV_TBL(PROV_ID);
ALTER TABLE TPL_DEF_TBL ADD CONSTRAINT FK_TPL_DEF_UPD_PROV_ID FOREIGN KEY (UPD_PROV_ID) REFERENCES SEC_PROV_TBL(PROV_ID);
ALTER TABLE TPL_DEF_TBL ADD CONSTRAINT FK_TPL_DEF_OBSLT_PROV_ID FOREIGN KEY (OBSLT_PROV_ID) REFERENCES SEC_PROV_TBL(PROV_ID);
 
--#!

ALTER TABLE ACT_VRSN_TBL RENAME CRT_USR_ID TO CRT_PROV_ID;
ALTER TABLE ACT_VRSN_TBL RENAME OBSLT_USR_ID TO OBSLT_PROV_ID;
ALTER TABLE ACT_VRSN_TBL DROP CONSTRAINT fk_act_vrsn_crt_usr_id;
ALTER TABLE ACT_VRSN_TBL DROP CONSTRAINT fk_act_vrsn_obslt_usr_id;
ALTER TABLE ACT_VRSN_TBL ADD CONSTRAINT FK_ACT_VRSN_CRT_PROV_ID FOREIGN KEY (CRT_PROV_ID) REFERENCES SEC_PROV_TBL(PROV_ID);
ALTER TABLE ACT_VRSN_TBL ADD CONSTRAINT FK_ACT_VRSN_OBSLT_PROV_ID FOREIGN KEY (OBSLT_PROV_ID) REFERENCES SEC_PROV_TBL(PROV_ID);

--#!

ALTER TABLE ACT_TAG_TBL RENAME CRT_USR_ID TO CRT_PROV_ID;
ALTER TABLE ACT_TAG_TBL RENAME OBSLT_USR_ID TO OBSLT_PROV_ID;
ALTER TABLE ACT_TAG_TBL DROP CONSTRAINT fk_act_tag_crt_usr_id;
ALTER TABLE ACT_TAG_TBL DROP CONSTRAINT fk_act_tag_obslt_usr_id;
ALTER TABLE ACT_TAG_TBL ADD CONSTRAINT FK_ACT_TAG_CRT_PROV_ID FOREIGN KEY (CRT_PROV_ID) REFERENCES SEC_PROV_TBL(PROV_ID);
ALTER TABLE ACT_TAG_TBL ADD CONSTRAINT FK_ACT_TAG_OBSLT_PROV_ID FOREIGN KEY (OBSLT_PROV_ID) REFERENCES SEC_PROV_TBL(PROV_ID);

--#!

ALTER TABLE ENT_TAG_TBL RENAME CRT_USR_ID TO CRT_PROV_ID;
ALTER TABLE ENT_TAG_TBL RENAME OBSLT_USR_ID TO OBSLT_PROV_ID;
ALTER TABLE ENT_TAG_TBL DROP CONSTRAINT fk_ent_tag_crt_usr_id;
ALTER TABLE ENT_TAG_TBL DROP CONSTRAINT fk_ent_tag_obslt_usr_id;
ALTER TABLE ENT_TAG_TBL ADD CONSTRAINT FK_ENT_TAG_CRT_PROV_ID FOREIGN KEY (CRT_PROV_ID) REFERENCES SEC_PROV_TBL(PROV_ID);
ALTER TABLE ENT_TAG_TBL ADD CONSTRAINT FK_ENT_TAG_OBSLT_PROV_ID FOREIGN KEY (OBSLT_PROV_ID) REFERENCES SEC_PROV_TBL(PROV_ID);

--#!

ALTER TABLE ENT_VRSN_TBL RENAME CRT_USR_ID TO CRT_PROV_ID;
ALTER TABLE ENT_VRSN_TBL RENAME OBSLT_USR_ID TO OBSLT_PROV_ID;
ALTER TABLE ENT_VRSN_TBL DROP CONSTRAINT fk_ent_vrsn_crt_usr_id;
ALTER TABLE ENT_VRSN_TBL DROP CONSTRAINT fk_ent_vrsn_obslt_usr_id;
ALTER TABLE ENT_VRSN_TBL ADD CONSTRAINT FK_ENT_VRSN_CRT_PROV_ID FOREIGN KEY (CRT_PROV_ID) REFERENCES SEC_PROV_TBL(PROV_ID);
ALTER TABLE ENT_VRSN_TBL ADD CONSTRAINT FK_ENT_VRSN_OBSLT_PROV_ID FOREIGN KEY (OBSLT_PROV_ID) REFERENCES SEC_PROV_TBL(PROV_ID);

--#!

ALTER TABLE PROTO_HDLR_TBL RENAME CRT_USR_ID TO CRT_PROV_ID;
ALTER TABLE PROTO_HDLR_TBL RENAME OBSLT_USR_ID TO OBSLT_PROV_ID;
ALTER TABLE PROTO_HDLR_TBL DROP CONSTRAINT fk_proto_hdlr_crt_usr_id;
ALTER TABLE PROTO_HDLR_TBL DROP CONSTRAINT fk_proto_hdlr_obslt_usr_id;
ALTER TABLE PROTO_HDLR_TBL ADD CONSTRAINT FK_PROTO_HDLR_CRT_PROV_ID FOREIGN KEY (CRT_PROV_ID) REFERENCES SEC_PROV_TBL(PROV_ID);
ALTER TABLE PROTO_HDLR_TBL ADD CONSTRAINT FK_PROTO_HDLR_OBSLT_PROV_ID FOREIGN KEY (OBSLT_PROV_ID) REFERENCES SEC_PROV_TBL(PROV_ID);

--#!

ALTER TABLE PROTO_TBL RENAME CRT_USR_ID TO CRT_PROV_ID;
ALTER TABLE PROTO_TBL RENAME OBSLT_USR_ID TO OBSLT_PROV_ID;
ALTER TABLE PROTO_TBL DROP CONSTRAINT fk_proto_crt_usr_id;
ALTER TABLE PROTO_TBL DROP CONSTRAINT fk_proto_obslt_usr_id;
ALTER TABLE PROTO_TBL ADD CONSTRAINT FK_PROTO_CRT_PROV_ID FOREIGN KEY (CRT_PROV_ID) REFERENCES SEC_PROV_TBL(PROV_ID);
ALTER TABLE PROTO_TBL ADD CONSTRAINT FK_PROTO_OBSLT_PROV_ID FOREIGN KEY (OBSLT_PROV_ID) REFERENCES SEC_PROV_TBL(PROV_ID);

--#!

ALTER TABLE ALRT_TBL RENAME CRT_USR_ID TO CRT_PROV_ID;
ALTER TABLE ALRT_TBL RENAME UPD_USR_ID TO UPD_PROV_ID;
ALTER TABLE ALRT_TBL RENAME OBSLT_USR_ID TO OBSLT_PROV_ID;
ALTER TABLE ALRT_TBL DROP CONSTRAINT fk_alrt_crt_usr_id;
ALTER TABLE ALRT_TBL DROP CONSTRAINT fk_alrt_obslt_usr_id;
ALTER TABLE ALRT_TBL DROP CONSTRAINT fk_alrt_upd_usr_id;
ALTER TABLE ALRT_TBL ADD CONSTRAINT FK_ALRT_CRT_PROV_ID FOREIGN KEY (CRT_PROV_ID) REFERENCES SEC_PROV_TBL(PROV_ID);
ALTER TABLE ALRT_TBL ADD CONSTRAINT FK_ALRT_UPD_PROV_ID FOREIGN KEY (UPD_PROV_ID) REFERENCES SEC_PROV_TBL(PROV_ID);
ALTER TABLE ALRT_TBL ADD CONSTRAINT FK_ALRT_OBSLT_PROV_ID FOREIGN KEY (OBSLT_PROV_ID) REFERENCES SEC_PROV_TBL(PROV_ID);

--#!

-- SECURITY SESSIONS TABLE
CREATE TABLE SEC_SES_TBL (
	SES_ID UUID NOT NULL DEFAULT uuid_generate_v1(),
	CRT_UTC TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP, -- THE TIME WHEN THE SESSION WAS ESTABLISHED
	EXP_UTC TIMESTAMPTZ NOT NULL, -- THE TIME WHEN THE SESSION WILL EXPIRE
	APP_ID UUID NOT NULL, -- THE SECURITY APPLICATION WHICH THE SESSION IS ESTABLISHED FOR
	USR_ID UUID, -- THE USER IDENTIFIER (APPLICATION USER IF APPLICATION GRANT)
	DEV_ID UUID,
	RFRSH_TKN VARCHAR(128) UNIQUE NOT NULL, -- THE REFRESH TOKEN FOR THE OBJECT 
	RFRSH_EXP_UTC TIMESTAMPTZ NOT NULL, -- THE TIME THAT THE REFRESH TOKEN EXPIRES
	AUD VARCHAR(32) NOT NULL, -- THE REMOTE IP ADDRESS THAT ESTABLISHED THE SESSION
	CONSTRAINT PK_SEC_SES_TBL PRIMARY KEY (SES_ID),
	CONSTRAINT FK_SEC_SES_APP_ID FOREIGN KEY (APP_ID) REFERENCES SEC_APP_TBL(APP_ID),
	CONSTRAINT FK_SEC_SES_USR_ID FOREIGN KEY (USR_ID) REFERENCES SEC_USR_TBL(USR_ID),
	CONSTRAINT FK_SEC_SES_DEV_ID FOREIGN KEY (DEV_ID) REFERENCES SEC_DEV_TBL(DEV_ID),
	CONSTRAINT CK_SEC_SES_EXP CHECK (EXP_UTC > CRT_UTC),
	CONSTRAINT CK_SEC_SES_RFRSH_EXP CHECK (RFRSH_EXP_UTC > EXP_UTC)
);

-- SECURITY SESSION CLAIMS TABLE
CREATE TABLE SEC_SES_CLM_TBL ( 
	CLM_ID UUID NOT NULL DEFAULT uuid_generate_v1(),
	SES_ID UUID NOT NULL, -- THE SESSION TO WHICH THE CLAIM BELONGS
    CLM_TYP VARCHAR(128) NOT NULL, -- THE TYPE OF CLAIM
    CLM_VAL VARCHAR(128) NOT NULL, -- THE VALUE OF THE CLAIM
	CONSTRAINT PK_SEC_SES_CLM_TBL PRIMARY KEY (CLM_ID),
	CONSTRAINT FK_SEC_SES_CLM_SES_ID FOREIGN KEY (SES_ID) REFERENCES SEC_SES_TBL(SES_ID)
);

--#!


INSERT INTO SEC_APP_TBL (APP_PUB_ID, APP_SCRT, CRT_PROV_ID)
	VALUES ('org.santedb.disconnected_client', ('ec1e5ef79b95cc1e8a5dec7492b9eb7e2b413ad7a45c5637d16c11bb68fcd53c'), 'fadca076-3690-4a6e-af9e-f1cd68e8c7e8');

INSERT INTO SEC_APP_POL_ASSOC_TBL(APP_ID, POL_ID, POL_ACT)
	SELECT APP_ID, POL_ID, 2 FROM
		SEC_APP_TBL, SEC_POL_TBL
	WHERE
		SEC_APP_TBL.APP_PUB_ID = 'org.santedb.disconnected_client';

INSERT INTO SEC_APP_TBL (APP_ID, APP_PUB_ID, APP_SCRT, CRT_UTC, CRT_PROV_ID) 
		VALUES ('4C5A581C-A6EE-4267-9231-B0D3D50CC08B', 'org.santedb.debug', 'cba830db9a6f5a4b638ff95ef70e98aa82d414ac35b351389024ecb6be40ebf0', CURRENT_TIMESTAMP, 'fadca076-3690-4a6e-af9e-f1cd68e8c7e8');


INSERT INTO SEC_APP_POL_ASSOC_TBL(APP_ID, POL_ID, POL_ACT)
	SELECT APP_ID, POL_ID, 2 FROM
		SEC_APP_TBL, SEC_POL_TBL
	WHERE
		SEC_APP_TBL.APP_PUB_ID = 'org.santedb.debug';
		
INSERT INTO SEC_APP_TBL (APP_ID, APP_PUB_ID, APP_SCRT, CRT_UTC, CRT_PROV_ID) 
		VALUES ('064C3DBD-8F88-4A5D-A1FA-3C3A542B5E98', 'org.santedb.administration', '59ff5973691ff75f8baa45f1e38fae24875f77ef00987ed22b02df075fb144f9', CURRENT_TIMESTAMP, 'fadca076-3690-4a6e-af9e-f1cd68e8c7e8');

INSERT INTO SEC_APP_POL_ASSOC_TBL(APP_ID, POL_ID, POL_ACT)
	SELECT APP_ID, POL_ID, 2 FROM
		SEC_APP_TBL, SEC_POL_TBL
	WHERE
		SEC_APP_TBL.APP_PUB_ID = 'org.santedb.administration';

	
INSERT INTO SEC_APP_TBL (APP_ID, APP_PUB_ID, APP_SCRT, CRT_UTC, CRT_PROV_ID) 
		VALUES ('B7ECA9F3-805E-4BE9-A5C7-30E6E495939B', 'org.santedb.disconnected_client.win32', 'd4f8cf183812156e561d390902c092fa4d1b08001ff875a4bd349ed56e1f31d4', CURRENT_TIMESTAMP, 'fadca076-3690-4a6e-af9e-f1cd68e8c7e8');

INSERT INTO SEC_APP_POL_ASSOC_TBL(APP_ID, POL_ID, POL_ACT)
	SELECT APP_ID, POL_ID, 2 FROM
		SEC_APP_TBL, SEC_POL_TBL
	WHERE
		SEC_APP_TBL.APP_PUB_ID = 'org.santedb.disconnected_client.win32';


CREATE OR REPLACE FUNCTION IS_USR_LOCK(
	USR_NAME_IN IN TEXT
) RETURNS BOOLEAN AS 
$$
BEGIN
	RETURN (SELECT (LOCKED > CURRENT_TIMESTAMP) FROM SEC_USR_TBL WHERE USR_NAME = USR_NAME_IN);
END
$$ LANGUAGE PLPGSQL;


DROP FUNCTION AUTH_USR(TEXT, TEXT, INTEGER);

-- AUTHENTICATES THE USER IF APPLICABLE
CREATE OR REPLACE FUNCTION AUTH_USR (
	USR_NAME_IN IN TEXT,
	PASSWD_IN IN TEXT,
	MAX_FAIL_LOGIN_IN IN INT
) RETURNS TABLE (
    USR_ID UUID,
    CLS_ID UUID,
    USR_NAME VARCHAR(64),
    EMAIL VARCHAR(256),
    EMAIL_CNF BOOLEAN,
    PHN_NUM VARCHAR(128), 
    PHN_CNF BOOLEAN,
    TFA_ENABLED BOOLEAN,
    LOCKED TIMESTAMPTZ, -- TRUE IF THE ACCOUNT HAS BEEN LOCKED
    PASSWD VARCHAR(128),
    SEC_STMP VARCHAR(128),
    FAIL_LOGIN INT,
    LAST_LOGIN_UTC TIMESTAMPTZ,
    CRT_UTC TIMESTAMPTZ,
    CRT_PROV_ID UUID, 
    OBSLT_UTC TIMESTAMPTZ,
    OBSLT_PROV_ID UUID, 
    UPD_UTC TIMESTAMPTZ,
    UPD_PROV_ID UUID, 
    ERR_CODE VARCHAR(128)
) AS $$
DECLARE
	USR_TPL SEC_USR_TBL;
BEGIN
	SELECT INTO USR_TPL * 
		FROM SEC_USR_TBL
		WHERE LOWER(SEC_USR_TBL.USR_NAME) = LOWER(USR_NAME_IN)
		AND SEC_USR_TBL.OBSLT_UTC IS NULL;

	IF (IS_USR_LOCK(USR_NAME_IN)) THEN
		USR_TPL.LOCKED = COALESCE(USR_TPL.LOCKED, CURRENT_TIMESTAMP) + ((USR_TPL.FAIL_LOGIN - MAX_FAIL_LOGIN_IN) ^ 1.5 * '30 SECONDS'::INTERVAL);
		UPDATE SEC_USR_TBL SET FAIL_LOGIN = SEC_USR_TBL.FAIL_LOGIN + 1, LOCKED = USR_TPL.LOCKED
			WHERE SEC_USR_TBL.USR_NAME = USR_NAME_IN;
		RETURN QUERY SELECT USR_TPL.*, ('AUTH_LCK:' || ((USR_TPL.LOCKED - CURRENT_TIMESTAMP)::TEXT))::VARCHAR;
	ELSE
		
		-- LOCKOUT ACCOUNTS
		IF (USR_TPL.PASSWD = PASSWD_IN) THEN
			UPDATE SEC_USR_TBL SET 
				FAIL_LOGIN = 0,
				LAST_LOGIN_UTC = CURRENT_TIMESTAMP,
				UPD_PROV_ID = 'fadca076-3690-4a6e-af9e-f1cd68e8c7e8',
				UPD_UTC = CURRENT_TIMESTAMP
			WHERE LOWER(SEC_USR_TBL.USR_NAME) = LOWER(USR_NAME_IN);
			RETURN QUERY SELECT USR_TPL.*, NULL::VARCHAR LIMIT 1;
		ELSIF(USR_TPL.FAIL_LOGIN > MAX_FAIL_LOGIN_IN) THEN 
			USR_TPL.LOCKED = COALESCE(USR_TPL.LOCKED, CURRENT_TIMESTAMP) + ((USR_TPL.FAIL_LOGIN - MAX_FAIL_LOGIN_IN) ^ 1.5 * '30 SECONDS'::INTERVAL);
			UPDATE SEC_USR_TBL SET FAIL_LOGIN = COALESCE(SEC_USR_TBL.FAIL_LOGIN, 0) + 1, LOCKED = USR_TPL.LOCKED
				WHERE SEC_USR_TBL.USR_NAME = USR_NAME_IN;
			RETURN QUERY SELECT USR_TPL.*, ('AUTH_LCK:' || ((USR_TPL.LOCKED - CURRENT_TIMESTAMP)::TEXT))::VARCHAR;
		ELSIF (USR_TPL.TFA_ENABLED) THEN
			UPDATE SEC_USR_TBL SET FAIL_LOGIN = COALESCE(SEC_USR_TBL.FAIL_LOGIN, 0) + 1 WHERE SEC_USR_TBL.USR_NAME = USR_NAME_IN;
			RETURN QUERY SELECT USR_TPL.*, ('AUTH_TFA:' || USR_NAME_IN)::VARCHAR;
		ELSE
			UPDATE SEC_USR_TBL SET FAIL_LOGIN = COALESCE(SEC_USR_TBL.FAIL_LOGIN, 0) + 1 WHERE SEC_USR_TBL.USR_NAME = USR_NAME_IN;
			RETURN QUERY SELECT USR_TPL.*, ('AUTH_INV:' || USR_NAME_IN)::VARCHAR;
		END IF;
	END IF;
END	
$$ LANGUAGE PLPGSQL;

DROP FUNCTION AUTH_APP(TEXT, TEXT);

-- AUTHENTICATE AN APPICATION
CREATE OR REPLACE FUNCTION AUTH_APP (
	APP_PUB_ID_IN IN TEXT,
	APP_SCRT_IN IN TEXT
) RETURNS SETOF SEC_APP_TBL AS 
$$ 
BEGIN
	RETURN QUERY (SELECT * FROM SEC_APP_TBL WHERE APP_PUB_ID = APP_PUB_ID_IN AND APP_SCRT = APP_SCRT_IN LIMIT 1);
END
$$ LANGUAGE PLPGSQL;

DROP FUNCTION AUTH_DEV(TEXT, TEXT);

-- AUTHENTICATE A DEVICE
CREATE OR REPLACE FUNCTION AUTH_DEV (
	DEV_PUB_ID_IN TEXT,
	DEV_SCRT_IN IN TEXT
) RETURNS SETOF SEC_DEV_TBL AS 
$$ 
BEGIN
	RETURN QUERY (SELECT * FROM SEC_DEV_TBL WHERE DEV_PUB_ID = DEV_PUB_ID_IN AND DEV_SCRT = DEV_SCRT_IN LIMIT 1);
END
$$ LANGUAGE PLPGSQL;	