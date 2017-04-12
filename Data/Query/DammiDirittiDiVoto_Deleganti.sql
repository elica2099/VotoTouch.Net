SELECT DISTINCT 
	A.CoAz, A.IdAzion, D.ProgDeleg, 
	CASE WHEN A.FisGiu ='F' THEN A.Cognome+ ' ' + A.Nome ELSE A.Raso END as Raso1,
	isnull(C.IDAzion, -1) as ConIdAzion, isnull(C.NumVotaz, -1) as ConIDVotaz,
	isnull(COALESCE(D.Voti1Ord,0)+COALESCE(D.Voti2Ord,0), 0) AS VtOrd,
	isnull(COALESCE(D.Voti1Str,0)+COALESCE(D.Voti2Str,0), 0) AS VtStr

FROM         
	GEAS_Deleganti as D WITH (NOLOCK) INNER JOIN GEAS_Anagrafe as A WITH (nolock) ON D.IdAzion = A.IdAzion
        left join VS_conschede as C WITH (nolock) on D.IDAzion = C.IdAzion and C.NumVotaz = @IDVotaz

WHERE  (D.Badge = @Badge) AND (D.Reale = 1)  ORDER BY D.ProgDeleg