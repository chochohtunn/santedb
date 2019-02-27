﻿/** 
 * <feature scope="SanteDB.Persistence.Data.ADO" id="0-003" name="Core Functions" invariantName="npgsql">
 *	<summary>Install Core Functions</summary>
 *	<remarks>Installs the core SanteDB database functions</remarks>
 *  <isInstalled>SELECT IS_USR_LOCK('SYSTEM') IS NULL;</isInstalled>
 * </feature>
 */
 -- RETURNS WHETHER THE USER ACCOUNT IS LOCKED
CREATE OR REPLACE FUNCTION IS_USR_LOCK(
	USR_NAME_IN IN TEXT
) RETURNS BOOLEAN AS 
$$
BEGIN
	RETURN (SELECT (LOCKED > CURRENT_TIMESTAMP) FROM SEC_USR_TBL WHERE USR_NAME = USR_NAME_IN);
END
$$ LANGUAGE PLPGSQL;


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

-- GET THE SCHEMA VERSION
CREATE OR REPLACE FUNCTION GET_SCH_VRSN() RETURNS VARCHAR(10) AS
$$
BEGIN
	RETURN '0.9.0.0';
END;
$$ LANGUAGE plpgsql;