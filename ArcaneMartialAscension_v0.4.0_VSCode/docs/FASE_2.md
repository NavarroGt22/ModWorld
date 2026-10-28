# Fase 2 — primeira entrega, 28/10/2026

## Status da classe e técnicas

A aba da espada passa a exibir Classe marcial: classe da arma selecionada,
equipamento, grau, próximo grau, XP e quantidade de técnicas conhecidas.
Logo abaixo fica a lista de técnicas aprendidas, com ícone, nome e nível
romano I–III à direita, seguindo a organização da referência fornecida.
Selecionar uma linha mostra efeito, potência, recarga, usos confirmados e origem.

A prévia de conteúdo futuro fica em Próximos treinos. Ela não aumenta o
contador de técnicas conhecidas. Consultar a ficha continua sem conceder
XP ou conhecimento. A arma equipada é selecionada ao trocar de personagem;
sem arma, a ficha seleciona a disciplina com mais XP.

## Separação das progressões

- Grau da arma: níveis 0–7, com os mesmos limiares de XP anteriores.
- Técnica aprendida: conhecimento individual com origem registrada.
- Domínio da técnica: I inicialmente; II após 5 usos com dano confirmado;
  III após 20. Não aumenta somente por subir o grau da arma.
- Aura: continua separada e não é exigida pelas quatro técnicas básicas.

Treino/prática/acertos que alcançam nível 1 ensinam a técnica básica da arma:
Corte preciso, Estocada precisa, Tiro preciso ou Golpe pesado. As outras
12 técnicas permanecem planejadas, inclusive em personagens de nível 7.
O efeito básico é dano adicional após um acerto, com recarga própria e arma
correspondente equipada. Não há combos, aparo, chuva de flechas ou ondas de
aura implementados por esta entrega.

## Acertos e armas

`WeaponHitObserver` usa o Harmony já instalado pelo NML. Observa o HP antes
e depois de `Actor.getHit/7`, apenas para `AttackType.Weapon`. Tentativas de
ataque via traços não concedem mais XP nem dano extra improvisado. O limite
de uma recompensa por frame/ator protege callbacks e impactos repetidos.
Dano gerado pelo AMA fica isolado para evitar recursão/XP pela própria magia.

`Projectile.start` registra a arma do lançador. Um contexto em
`Projectile.checkHitForUnit` mantém essa identidade no acerto, mesmo que o
personagem troque de equipamento antes da flecha chegar. Nesse caso, o XP
pertence à arma lançadora, e o bônus da técnica exige que ela ainda esteja equipada.
O registro usa referências fracas e é limpo na troca de mundo.

APIs conferidas na DLL local: `Actor.getHit(float,bool,AttackType,BaseSimObject,bool,bool,bool)`,
`Projectile.start(BaseSimObject,BaseSimObject,Vector3,Vector3,string,float,float,float,Action,Kingdom)`
e `Projectile.checkHitForUnit(BaseSimObject,AttackData)`. O CIL de
`Actor.tryToAttack` constrói `AttackData` com tipo Weapon (12). Harmony local:
2.9.0.0; nenhuma DLL foi adicionada ao repositório.
Na DLL original, `getHit` é interno (`Assembly`), enquanto a DLL publicizada
o apresenta como público. A busca do patch usa as duas visibilidades;
essa diferença foi identificada na primeira inicialização e corrigida.

## Motor de magias

`SpellRuntime.cs` define efeitos, perfis explícitos, pedido, resultado,
ticket de reserva, sessão de conjuração e interface do executor. A definição
básica de magia permanece em `Core/ArcaneProgression.cs`.

O catálogo permanece com 160 entradas. Disponibilidade não é inferida apenas
da categoria: existem 11 perfis ativos, dos círculos 0 e 1. O estado distingue
catalogada, protótipo parcial, implementada e testada. Nenhuma magia é marcada
testada automaticamente por um efeito ter sido executado.

Componentes reutilizáveis ativos: dano direto, entrega por projétil, dano em
área limitado, cura/drenagem e atributos temporários. Escudo de absorção,
invocação e teleporte têm tipos definidos, mas continuam indisponíveis até
terem adaptadores próprios. Detecção/controle/ilusão também aguardam suas
mecânicas; não são substituídos por um bônus genérico sem relação com a descrição.

Estilhaço Glacial e Toque Frio ainda não aplicam lentidão. Rajada de Cinzas
usa área circular limitada, sem cone. Esses três perfis são protótipos
parciais e exibem a limitação na ficha.

Pedido e resolução validam identidade, vida, círculo, escola, mana, alvo,
alcance, pausa e recarga. Um ticket só pode finalizar uma vez. Mana, XP e
recarga só são confirmados após efeito bem-sucedido. A recarga, regeneração
de mana e expiração dos atributos usam tempo de simulação. Os VFX ficam fora
da transação de custo. Um traço já permanente não passa a ser removido ao
expirar uma magia temporária.

## Validação

- 22 fontes compilados com DLLs reais, zero erros/avisos.
- 58 cenários automatizados aprovados; 27 novos casos da Fase 2.
- Catálogo de 160 magias, 16 ícones e traduções dos 49 traços validados.
- Os testes de regras não executam a simulação nem confirmam Harmony/combate.
- No WorldBox, registro dos 49 traços e do observador de acertos confirmado.
- Personagem Darocar: dois treinos manuais (+15 XP cada), grau Aprendiz,
  XP 30/100, contador de uma técnica e linha Corte preciso I observados na
  mesma área de status. O teste manual não valida XP por ataques naturais.
- Uma tentativa de conjuração sem alvo foi rejeitada com mana 100/100 preservada.
- Seleção por F7/F8 passou a usar `MapBox.getMousePos/getActorNearCursor`
  e `Actor.current_position`, conferidos na DLL local, em lugar de exigir
  transform de renderização atualizado/projeção de profundidade positiva.
- Os VFX de entidades também usam a posição lógica estável do mapa.
- Após o ajuste, F7 selecionou Ninde pelo cursor. A tentativa com o jogo
  pausado foi rejeitada com mana preservada. Em seguida, uma conjuração manual
  válida de Centelha Rubra acertou Eawurg (ator 31): HP 73 → 66, mana 100 → 97,
  recarga registrada de 2 s. O log contém `launched`, `applied` e `result`.
- O diagnóstico continuou com XP de espada 0 após a magia; o efeito não
  treinou a arma. Não houve exceção nova de integração nesse teste.

Trecho do log de impacto:

`[AMA Cast] applied spell=ama.spell.evocation.0.centelha_rubra caster=5 target=31 hp=73->66 damage=7 mana=97 cooldown=2`

Isso confirma um caso manual no motor novo. Não é validação do combate
automático, da duração da recarga sob todas as velocidades ou das outras
magias. O estado global das magias não foi marcado como Testada por esse caso.

Backups antes do teste:

- Saves: `C:\Users\rn558\Documents\AMA_TestBackups\Phase2_20261028_173509`.
- Mod anterior: `C:\Program Files (x86)\Steam\steamapps\common\worldbox\AMA_Backups\ArcaneMartialAscension_20261028_173510`.
- Antes da correção de visibilidade: `C:\Program Files (x86)\Steam\steamapps\common\worldbox\AMA_Backups\ArcaneMartialAscension_20261028_173845`.
- Antes da seleção nativa: `C:\Program Files (x86)\Steam\steamapps\common\worldbox\AMA_Backups\ArcaneMartialAscension_20261028_174904`.

O log da falha inicial está em `Player_before_observer_lookup_fix.log` no backup.
`Player_before_native_cursor_fix.log` preserva o teste da classe e a rejeição
da tentativa sem alvo. Nenhum desses testes salvou por cima de mundos do usuário.
`Player_phase2_final.log` preserva a carga final, seleção por F7, rejeição
durante pausa, lançamento e impacto com HP/mana medidos. Os 22 fontes e as
duas traduções instaladas foram conferidos por hash contra o workspace;
o mod instalado não contém DLLs. O jogo ficou fechado após os testes.

## Próximos aceites

Validar ataques reais e progressão para as quatro armas, incluindo troca de
arma durante voo, uso automático das técnicas, expiração de bônus e carga
com muitas unidades. A persistência de XP, técnicas e mana ainda pertence
ao trabalho pendente. Esta entrega inicia a Fase 2; não a declara concluída.

## Expansão solicitada — aptidão natural por criatura

Em 28/10/2026, o usuário propôs círculos elevados para dragões e magia e
maestria de espada para demônios, como exemplos de perfis por espécie.
O requisito está registrado na seção 6.3 do roadmap completo, ainda sem
implementação ou valores de balanceamento definidos. O observador atual
continua restrito a habitantes sapientes adultos; o currículo automático
continua limitado a quatro magias de Evocação dos círculos 0–1.

O estudo próprio de Centelha foi observado no jogo. O seletor de magia
automático acompanha o alvo de combate e verifica conhecimentos, escola,
círculo, mana, alcance e recarga, mas o ciclo completo de aprender e
conjurar em uma luta real ainda precisa de validação. O impacto com dano
confirmado nesta entrega foi disparado manualmente com F8.

Atualização posterior: a expansão de formação natural confirmou aprendizado
e conjuração automática por Magos, Bruxos e Feiticeiro contra ursos.
Ver `FORMACAO_NATURAL_E_FX.md` para a evidência e os limites desse aceite.
