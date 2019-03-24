SELECT DISTINCT 
	A.CoAz, A.IdAzion, D.ProgDeleg, isnull(A.CoFi, ''),
	CASE WHEN A.FisGiu ='F' THEN A.Cognome+ ' ' + A.Nome ELSE A.Raso END as Raso1,
	isnull(VS_conschede.IDAzion, -1) as ConIdAzion,
	COALESCE(D.Azioni1Ord,0)+COALESCE(D.Azioni2Ord,0) AS AzOrd,COALESCE(D.Azioni1Str,0)+COALESCE(D.Azioni2Str,0) AS AzStr

FROM         
	GEAS_Deleganti as D WITH (NOLOCK) INNER JOIN GEAS_Anagrafe as A WITH (nolock) ON D.IdAzion = A.IdAzion
        left join VS_conschede WITH (nolock) on D.IDAzion = VS_conschede.IdAzion

WHERE     (D.Badge = @Badge) AND (D.Reale = 1) ORDER BY D.ProgDeleg