# Atributos e formação — 28/10/2026

## Escopo

Referência de organização: nove prints de Combat Gods fornecidas pelo usuário.
Foram usados os ícones existentes do AMA. Nenhum código ou asset desse mod
foi copiado. Esta revisão não implementa seus feixes, combos, escolas de
combate, ranking mundial ou graus divinos.

A ficha mantém duas abas: Maestrias e Grimório. Formação arcana e escolas
aparecem numa mesma área, acima do catálogo. Círculo disponível é a capacidade
de aprendizado do ator; círculo aprendido é o maior círculo entre feitiços
conhecidos daquela escola. Ausência de conhecimento não é exibida como uma
magia de círculo zero. O filtro continua sendo apenas navegação do catálogo.

## Atributos

`Code/Core/TraitAttributes.cs` centraliza vida, dano, armadura e velocidade
de ataque para o registro nativo e a interface. São os mesmos identificadores
de stats usados na integração anterior. Falha em registrar um atributo desses
perfis gera erro explícito. O teste no jogo revelou que `base_stats` inicia
nulo e a escrita reflexiva anterior podia falhar silenciosamente. A correção
inicializa `new BaseStats()`, usa o indexador nativo e confere o valor gravado.

Verificação estática da DLL local, sem executar seu código: cadeia
`ActorTrait` → `BaseTrait<ActorTrait>` → `BaseAugmentationAsset` →
`BaseUnlockableAsset`, com campo herdado `BaseStats base_stats` não inicializado
pelos construtores; `BaseStats.Item[string]` tem getter/setter float. Os IDs
`health`, `damage`, `armor`, `attack_speed` estão em `BaseStatsLibrary.init`.
SHA-256 da DLL publicizada:
`8c54a0b93a3b929834fb426fce8ce66344444e7709484956ddf04267de718234`;
MVID `eaf20368-35e7-4bb6-b082-5477d61019a6`.
Graus e buffs também desativam a concessão aleatória/nascimento pelos campos
nativos `spawn_random_trait_allowed` e `rate_birth`, verificados na mesma build.

- Espada equilibra dano, armadura e velocidade de ataque.
- Lança prioriza armadura e controle, com dano e cadência.
- Arco prioriza velocidade de ataque e dano.
- Machado prioriza dano e vitalidade.
- Tradições e escolas têm combinações próprias desses atributos.

Os níveis 1–7 recebem traços de grau próprios, com nomes traduzidos e um grupo
dedicado no editor. Somente o grau atual soma seus bônus à disciplina da arma
equipada; graus anteriores e outras armas são removidos pela sincronização.
O nível 0 mantém apenas a disciplina. A tabela de XP permanece inalterada.
Os IDs antigos de graus 1–3 agora correspondem diretamente aos níveis 1–3;
não há migração de XP porque a progressão ainda permanece apenas em memória.

Mana continua sendo recurso próprio do AMA, com capacidade por círculo.
Os perfis não anunciam mana/vigor nativos, crítico, esquiva ou efeitos de
técnicas que ainda não possuem implementação. Técnicas da prévia continuam
marcadas como planejadas mesmo quando seu requisito de nível é alcançado.

## Verificações

- Compilação com DLLs reais de WorldBox 0.51.2/NML 1.2.1.1: 18 fontes,
  zero erros e avisos.
- Testes de regras: 31 aprovados, zero falhas. Incluem círculo conhecido
  versus capacidade, múltiplas escolas, leitura sem mutação, perfis distintos,
  crescimento dos sete graus e preservação da identidade de cada arma.
- Catálogo de 160 magias, 16 ícones e nomes/descrições dos 49 traços
  validados em português e inglês.
- Testes automatizados não executam a simulação do jogo.
- Após a correção, inicialização real no WorldBox confirmou:
  `[AMA Stats] 49 traços registrados com atributos nativos verificados.`
  Isso valida gravação/leitura dos valores na biblioteca; não mede dano de combate.

Instalado com o jogo fechado e backups:

- Saves: `C:\Users\rn558\Documents\AMA_TestBackups\Attributes_20261028_165124`.
- Mod anterior: `C:\Program Files (x86)\Steam\steamapps\common\worldbox\AMA_Backups\ArcaneMartialAscension_20261028_165127`.
- Instalação antes da correção de stats: `C:\Program Files (x86)\Steam\steamapps\common\worldbox\AMA_Backups\ArcaneMartialAscension_20261028_165758`.
- Instalação antes da correção das abas: `C:\Program Files (x86)\Steam\steamapps\common\worldbox\AMA_Backups\ArcaneMartialAscension_20261028_170419`.
- Antes dos últimos ajustes de resumo/ajuda: `C:\Program Files (x86)\Steam\steamapps\common\worldbox\AMA_Backups\ArcaneMartialAscension_20261028_171152`.

O AMA estava desativado no registro do NML; foi reativado para este teste,
com cópia de `mod_compile_records.json` no backup de saves e preservação das
opções dos outros mods. O log anterior à correção está nesse mesmo backup.

Ao abrir a ficha, o teste revelou uma corrida entre a remoção adiada do
`DragOrderElement` dos clones e `DragOrderContainer.updateChildrenData`.
Mesmo desabilitado, o componente era enumerado antes do fim do frame e
`getRect()` tentava usar `_container` nulo. O clone agora remove somente
esse componente com `DestroyImmediate`, antes de voltar ao loop nativo.
`Player_before_tab_fix.log` preserva a falha observada.

## Conferência no WorldBox

Após a correção das abas, foi criado um humano em um mapa gerado para teste,
sem carregar ou salvar por cima de um mundo do usuário. A personagem Noroine
(ator 5) aprendeu espontaneamente Centelha Rubra. O log registrou:

`[AMA Learning] learned actor=5 spell=ama.spell.evocation.0.centelha_rubra source=self_study worldTime=48,9599989056587`

Foram observados na ficha:

- Formação arcana com Mago, círculo 0/9, mana 100/100 e uma magia aprendida.
- Evocação com C0 e uma magia; escolas restantes como não estudadas.
- Alternância para Maestrias, arma ausente, nível 0/7, próximo grau Aprendiz,
  XP 0/30 e indicação de bônus inativos.
- Atributos da espada: dano +3, armadura +1, velocidade de ataque +4;
  grau 0 sem bônus adicionais.
- Ausência de nova `NullReferenceException` ao abrir e alternar as abas.

Após essa conferência, o resumo recebeu a linha de escolas conhecidas no topo
e os botões tiveram os textos herdados do editor de equipamento substituídos
pelos textos próprios. Estes últimos ajustes compilaram sem erros/avisos e
foram instalados com o jogo fechado, sem uma nova abertura para conferência.
`Player_after_tab_fix.log` no backup preserva a observação anterior.
Os hashes dos 18 fontes e das duas traduções instaladas coincidem com o
workspace; nenhuma DLL foi copiada para o mod. O WorldBox ficou fechado
após o teste, pronto para recompilar os últimos ajustes na próxima abertura.

Esse resultado confirma um caso de estudo espontâneo no jogo, sem validar
todo o currículo, a prática com armas ou grandes populações. Dano/custos reais,
balanceamento sob carga e persistência continuam pendentes de aceite específico.
A Fase 1 do roadmap não está concluída.
