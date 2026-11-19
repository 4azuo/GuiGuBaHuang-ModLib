# CustomSkillEvent (disabled)

`3161035078/ModProject/ModCode/ModMain/Mod/CustomSkillEvent.cs` - cache key `ModConst.CUSTOM_SKILL_EVENT` - group: Battle

**The whole file is commented out**: not compiled, no effect in game. It is the start of a "custom skill" editor:

- a "Custom Skill" button on the battle info screen (`UICover<UIBattleInfo>`, page 1);
- page 2: select the martial type (left skill, right skill, step, ultimate) and an effect read from `ModConf/_Cus_Res_Efx_Battle_Skill.json` (`Object/CustomEffect.cs`), with Done / Cancel;
- would store `List<CustomSkill>` (`Object/CustomSkill.cs`).

`Ok()` does nothing yet. Open work: "Custom skills" in [05-Backlog](../05-Backlog.md).
