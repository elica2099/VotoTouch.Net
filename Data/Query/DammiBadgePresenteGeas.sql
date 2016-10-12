
DECLARE @MozioneReale as int
DECLARE @ProgMozione as int
DECLARE @ApVotDate as datetime
DECLARE @ValAssem as VarChar(3)

set @MozioneReale = (select isnull(GEAS_MatchVot.MozioneReale, -1) as MozioneReale from GEAS_MatchVot
					where GEAS_MatchVot.VotoSegretoDettaglio > 0)
set @ProgMozione = (select isnull(GEAS_MatchVot.ProgMozione, 0) as ProgMozione from GEAS_MatchVot
					where GEAS_MatchVot.MozioneReale = @MozioneReale)
set @ApVotDate = (select isnull(GEAS_Eventi.DataOra, 0) as ApVotDate from GEAS_Eventi
					where GEAS_Eventi.ProgMozione = @ProgMozione and GEAS_Eventi.CodEvento = '(APVO)')
set @ValAssem = (select isnull(GEAS_MatchVot.TipoAsse, 0) as TipoAssemblea from GEAS_MatchVot
					where GEAS_MatchVot.MozioneReale = @MozioneReale)

if @ValAssem = 'S'
begin
	set @ValAssem = 'O'
end
else
begin
	set @ValAssem = 'S'	
end

execute SP_BadgePresente @ApVotDate, @Badge, @ValAssem
