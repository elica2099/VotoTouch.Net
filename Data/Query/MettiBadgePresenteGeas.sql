
DECLARE @MozioneReale as int
DECLARE @ProgMozione as int
DECLARE @ApVotDate as datetime

set @MozioneReale = (select isnull(GEAS_MatchVot.MozioneReale, -1) as MozioneReale from GEAS_MatchVot
					where GEAS_MatchVot.VotoSegretoDettaglio > 0)
set @ProgMozione = (select isnull(GEAS_MatchVot.ProgMozione, 0) as ProgMozione from GEAS_MatchVot
					where GEAS_MatchVot.MozioneReale = @MozioneReale)
set @ApVotDate = (select isnull(GEAS_Eventi.DataOra, 0) as ApVotDate from GEAS_Eventi
					where GEAS_Eventi.ProgMozione = @ProgMozione and GEAS_Eventi.CodEvento = '(APVO)')

insert into Geas_TimbinOut with (ROWLOCK) 
	(DataOra, Badge, TipoMov, Reale, Classe, Terminale, DataIns)
	values (DATEADD(SECOND, -1, @ApVotDate), @Badge, 'E', 1, 99, @Sala, { fn NOW() })
insert into Geas_TimbinOut with (ROWLOCK) 
	(DataOra, Badge, TipoMov, Reale, Classe, Terminale, DataIns)
	values (DATEADD(SECOND, 1, @ApVotDate), @Badge, 'U', 1, 99, @Sala, { fn NOW() })

