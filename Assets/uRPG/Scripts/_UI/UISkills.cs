// Note: this script has to be on an always-active UI parent, so that we can
// always react to the hotkey.
using UnityEngine;
using UnityEngine.UI;

public class UISkills : MonoBehaviour
{
    public UISkillSlot slotPrefab;
    public Transform content;
    public Text skillExperienceText;

    void Update()
    {
        Player player = Player.player;
        if (!player) return;

        // instantiate/destroy enough slots
        // (we only care about non status skills)
        UIUtils.BalancePrefabs(slotPrefab.gameObject, player.skills.skills.Count, content);

        // refresh all
        for (int i = 0; i < player.skills.skills.Count; ++i)
        {
            UISkillSlot slot = content.GetChild(i).GetComponent<UISkillSlot>();
            Skill skill = player.skills.skills[i];

            bool isPassive = skill.data is PassiveSkill;

            // drag and drop name has to be the index in the real skill list,
            // not in the filtered list, otherwise drag and drop may fail
            slot.dragAndDropable.name = i.ToString();

            // click event
            slot.button.interactable = skill.level > 0 &&
                                       !isPassive &&
                                       skill.CanCast(player) && // checks mana, cooldown etc.
                                       !player.look.IsFreeLooking();
            int icopy = i;
            slot.button.onClick.SetListener(() => {
                // try use the skill or walk closer if needed
                player.skills.StartCast(icopy);
            });

            // set state
            slot.dragAndDropable.dragable = skill.level > 0 && !isPassive;

            // image
            if (skill.level > 0)
            {
                slot.image.color = Color.white;
                slot.image.sprite = skill.image;
            }

            // description
            slot.descriptionText.text = skill.ToolTip(showRequirements: skill.level == 0);

            // learn / upgrade
            if (skill.level < skill.maxLevel && player.skills.CanUpgrade(skill))
            {
                slot.upgradeButton.gameObject.SetActive(true);
                slot.upgradeButton.GetComponentInChildren<Text>().text = skill.level == 0 ? "Learn" : "Upgrade";
                slot.upgradeButton.onClick.SetListener(() => { player.skills.Upgrade(icopy); });
            }
            else slot.upgradeButton.gameObject.SetActive(false);

            // cooldown overlay
            float cooldown = skill.CooldownRemaining();
            slot.cooldownOverlay.SetActive(skill.level > 0 && cooldown > 0);
            slot.cooldownText.text = cooldown.ToString("F0");
            slot.cooldownCircle.fillAmount = skill.cooldown > 0 ? cooldown / skill.cooldown : 0;
        }

        // skill experience
        skillExperienceText.text = player.skills.skillExperience.ToString();
    }
}
