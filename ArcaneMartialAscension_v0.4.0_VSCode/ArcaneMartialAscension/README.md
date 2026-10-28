# Arcane & Martial Ascension v0.4.0 ALPHA

**Alvo:** WorldBox 0.51.2, NeoModLoader 1.2.1.1. Código-fonte C# compilado pelo carregador do jogo.

## O que esta versão realmente contém

- **160 feitiços no código C#:** `Code/Spells/SpellCatalog.cs` tem as 160 definições (o conteúdo foi gerado a partir do grimório previamente aprovado). O jogador **não instala magias individualmente** nem copia o JSON por fora.
- **Selecionar/aprender/lançar:** escola e magia selecionáveis no painel. Cada habitante tem feitiços aprendidos, experiência e mana próprios, somente na memória da sessão.
- **Efeitos jogáveis experimentais:** famílias de feitiços de círculo **0 e 1** com **dano**, **dano em área limitado**, **dreno**, **escudo**, **melhoria**, **controle/debuff básico**, **percepção/foco** e **ilusão defensiva**. A mecânica usa HP ou traços do jogo de forma experimental; não inclui toda a descrição rica individual de cada feitiço.
- **OUTRAS magias dos 160:** no catálogo e selecionáveis, mas marcadas **MECANICA PENDENTE**. **Não consomem mana** quando tentadas. Mecânicas de invocação, portal, necromancia avançada, teleporte, batalhões ilusórios e domínio ainda não estão implementadas.
- **Maestria por arma:** espada, lança, arco e machado. XP por ataques com traço (+2), treino manual (+15), níveis 0 a 7, nomes de classificação, traços de patamar em níveis 2/4/6. Tentativa de detectar arma equipada em NPCs a cada 4s em lotes de 150. Detecção depende do formato real dos dados de equipamento da build.
- **VFX de 2 formas:** tentativa de SpriteRenderer em coordenadas do mapa E um overlay visível via `OnGUI` (independe da câmera/map sorting layer), com círculo, pulso e corte, também quando a magia está indisponível para diagnosticar falhas.
- **Ataques normais de maestria:** traços armados acumulam XP e fornecem dano adicional de protótipo (não letal) em alguns acertos, além de tentar aplicar bônus de estatísticas.

## Limites IMPORTANTES

**Ainda não foi compilado nem testado dentro do WorldBox do usuário!** A estrutura/JSON/catálogo foram validados estaticamente. O carregamento e os efeitos dependem da API concreta do WorldBox 0.51.2.

1. **Mecânicas incompletas:** as 160 magias estão no código, mas somente **famílias básicas dos círculos 0 e 1** têm implementação experimental. Para feitiços avançados, nenhum custo de mana é feito.
2. **Dano não letal no fallback:** quando não encontra uma função segura de dano, a implementação tenta reduzir HP atual do personagem alvo, mas nunca abaixo de 1 HP. Não cria cadáver ou crédito de morte por si só e não substitui o pipeline de dano do jogo.
3. **Buffs por traço:** se a versão do jogo ignorar os stats dos traços do mod, o efeito terá traço visual/temporário sem bônus efetivo. Se `removeTrait(string)` não existir, a limpeza do efeito temporário pode falhar; envie o Player.log.
4. **Progressão não persistente:** XP, mana, aprendizados e cooldowns desaparecem ao fechar o jogo. Traços adicionados pelos sistemas normais do WorldBox podem ser salvos pelo jogo, mas isso não foi verificado nesta versão.
5. **Controle de NPCs:** o modo de detecção de armas tenta vincular as disciplinas automaticamente usando o equipamento, porém não inventa resultados se os metadados não forem reconhecidos; F9 permite garantir uma unidade de teste.
6. **Sem efeitos completos para cada tradição, sem invocação real, sem IA avançada e sem animações tipo Combat Gods** nesta build.
7. **F8 não tem integração com o mod de controle de unidades de terceiros:** é uma tecla de mod; o personagem é selecionado pelo painel/F7.

## Instalação (não sobrescrever sem limpar)

1. Feche o WorldBox.
2. Steam > WorldBox > Gerenciar > Explorar arquivos locais.
3. Abra `Mods` e **remova apenas** a pasta antiga `ArcaneMartialAscension` do nosso mod.
4. Extraia **a pasta `ArcaneMartialAscension`** do ZIP em `Mods`, sem uma pasta duplicada.
5. Confira que `Mods/ArcaneMartialAscension/mod.json` existe (não `Mods/ArcaneMartialAscension/ArcaneMartialAscension/mod.json`).
6. Inicie o WorldBox. No NML deve aparecer **Arcane & Martial Ascension v0.4.0**.

## Teste guiado: primeiro o desenho do círculo

1. Abra um mundo. Com o cursor sobre o mapa, aperte **F5** ou clique em **F5 VFX** no painel.
2. Deve aparecer um círculo **laranja sobre o cursor** por cerca de 1,5 s. Este teste não exige habitante, mana nem posição de mapa. Se falhar, envie o Player.log.
3. Crie dois habitantes e aproxime a câmera. Aponte para o primeiro e aperte **F7**; o painel deve mostrar o nome.
4. Mantenha a escola **Evocação**, feitiço **Centelha Rubra (círculo 0)**. Já vem aprendido no personagem de teste. Aponte o cursor para o **segundo habitante** e aperte **F8**. Se o alvo for encontrado, o feitiço tenta afetar o HP dele; deve surgir círculo na tela, e o painel indicará sucesso ou mensagem de falha. Se não houver alvo válido, o círculo visual ainda aparecerá, **mas não gastará mana**.
5. Use setas na interface para outras escolas/magias. Clique **Aprender magia selecionada** e depois use F8, mantendo o círculo exigido.
6. Para testar maestria, volte o cursor à unidade selecionada, escolha **Espada**, pressione F9 diversas vezes e confira o XP e nível. Clique nas setas para testar **Lança**, **Arco** e **Machado**. Ao obter níveis 2,4,6, traços extras de patamar são atribuídos.
7. Se quiser, deixe dois reinos em guerra para verificar a XP das armas dos NPCs que receberam os traços. Ainda pode haver limitações na detecção de equipamento.

## Controles

| Tecla | Função |
|---|---|
| F5 | Teste de círculo gráfico independente do mapa |
| F6 | Mostrar/ocultar painel |
| F7 | Selecionar habitante próximo ao cursor |
| F8 | Tentar lançar a magia selecionada |
| F9 | Treino manual da arma selecionada (+15 XP) |
| F10 | Despertar/manifesta aura |
| F11 | Alternar arma |
| F12 | Estudo de magia (+250 XP do mago) |

**Motivo da mudança de F8:** na v0.3.3 a chamada de conjuração retirava mana e acionava apenas um SpriteRenderer que pode não renderizar em WorldBox. A v0.4 adiciona o overlay `OnGUI` e não gasta mana sem uma ação aplicada com sucesso.

## Estrutura

- `Code/Spells/SpellCatalog.cs`: **catálogo executável em C#** (160).
- `Code/GameIntegration/ActorAbilities.cs`: aprendizado, casting, cooldown, treino e ações em ataques.
- `Code/GameIntegration/CombatBridge.cs`: mutações refletidas de HP / status (experimental).
- `Code/GameIntegration/GameWeaponDetector.cs`: reconhecimento de equipamento de NPCs.
- `Code/VisualEffects/ScreenEffects.cs`: desenho dos círculos na interface, independe de SpriteRenderer.
- `Code/VisualEffects/CombatVisualController.cs`: sprites alternativos para mapa.
- `Code/Core/CharacterProgression.cs`: XP, níveis e recursos.
- `Config/catalogo_magias_160.json`: cópia editável / documentação; **não é lido em execução**.

## Segurança do teste

Use um **mundo separado de testes** para esta versão alfa, já que os efeitos alteram vida e traços de personagens. Guarde uma cópia de seus mundos importantes. Se surgir `Failed to compile mod`, envie o `Player.log` completo e o `Player-prev.log` do teste. Não atualize o NML apenas por esse erro; ele pode ser do mod.
