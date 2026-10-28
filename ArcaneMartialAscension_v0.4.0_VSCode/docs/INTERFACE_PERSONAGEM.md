# Maestrias e Grimório na ficha do personagem

## Comportamento

Dois ícones entram na coluna lateral da `UnitWindow`:

- Espada: Maestrias. Mostra quatro armas, nível 0–7, título, XP, próximo
  limiar, barra de progresso e prévia de quatro técnicas por arma.
- Mago: Grimório. Organiza as 160 magias por oito escolas e círculos 0–9,
  duas magias por combinação. Exibe conhecimento, mana, recarga, descrição
  e a indicação de protótipo ou mecânica planejada.

As técnicas listadas são uma prévia de conteúdo: atingir o requisito não
ativa efeitos de combate ainda não implementados. Não há botão para executá-las.

Aprender uma magia chama o aprendizado existente; preparar seleciona o
conjurador e a magia para o atalho F8, sem conjuração nem consumo de mana.
O aprendizado é desabilitado sem habitante vivo ou com círculo insuficiente.
A preparação exige magia aprendida e com protótipo disponível.

`SelectedUnit.unit` é a fonte da seleção; foi confirmado nas DLLs locais que
`UnitWindow.actor` usa essa mesma propriedade. Abrir as abas não chama `State`
nem aplica talentos iniciais, XP, traços, pactos ou magias. O painel atualiza
quatro vezes por segundo enquanto está visível.

O F6 abre o diagnóstico antigo, oculto por padrão. A interface não adiciona
persistência. A revisão de conjuração seguinte passou a usar a câmera do mapa
no F8, exibir o resultado mesmo com F6 fechado e simular projéteis com impacto
confirmado; evidências em `VALIDACAO_CONJURACAO.md`.

## APIs verificadas na instalação local

- `UnitWindow._button_equipment_editor` e `WindowMetaTab.container`.
- `WindowMetaTab.tab_elements`, `tab_action`, `toggleActive` e `doAction`.
- `WindowMetaTabButtonsContainer.addTabContent`, `refillTabsWithContent`,
  `showTab` e `removeTab`.
- `ScrollWindow.transform_content` e `titleText`.
- `SpriteTextureLoader.getSprite(string)` para reutilizar os ícones do mod.

A integração respeita o estado de disponibilidade dos botões e o mecanismo
nativo de alternância de conteúdo. A prévia não inventa técnicas funcionais.

## Validação

- [x] Compilar com DLLs reais do WorldBox 0.51.2 e NML 1.2.1.1: 13 fontes,
  zero erros e avisos.
- [x] `git diff --check` sem erros.
- [ ] Instalar com o jogo fechado e verificar `[AMA UI]` no `Player.log`.
- [ ] Abrir ficha e verificar os dois novos ícones, sem sobrepor os existentes.
- [ ] Alternar Maestrias/Grimório e voltar às abas originais.
- [ ] Ver quatro armas e trocar a lista de técnicas ao escolher cada arma.
- [ ] Navegar entre as oito escolas e os círculos 0–9.
- [ ] Conferir leitura sem alteração de XP ou magias ao abrir a interface.
- [ ] Trocar de habitante e conferir que nome, nível e conhecimento acompanham a seleção.
- [ ] Conferir botões desabilitados para círculo insuficiente e magia pendente.
- [ ] Fechar/reabrir a ficha sem duplicar botões.

Testes de aprendizado ou conjuração devem usar mundo descartável, com backup.
