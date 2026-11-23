using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Dramon
{
    /// <summary>
    /// 결투 화면의 드래곤 그림. 대기 / 공격 / 피격 그림 전환, 등장할 때 어두운 색 → 원래 색, 울음소리.
    /// 그림만 바꾸고 위치·크기는 Scene 배치를 그대로 쓴다.
    /// </summary>
    public class DragonBattlerView : MonoBehaviour
    {
        [Header("연결")]
        [Tooltip("드래곤 그림을 그릴 Image")]
        [SerializeField] Image image;
        [Tooltip("울음소리를 낼 AudioSource (비워 두면 소리 없음)")]
        [SerializeField] AudioSource audioSource;

        [Header("연출")]
        [Tooltip("공격·피격 그림을 보여 준 뒤 대기 그림으로 돌아올 때까지의 시간(초)")]
        [Min(0f)]
        [SerializeField] float poseDuration = 0.4f;
        [Tooltip("등장 직후 어두운 색 (원래 색으로 돌아오기 전)")]
        [SerializeField] Color darkTint = new Color(0.25f, 0.25f, 0.32f, 1f);

        DragonSpecies species;
        Coroutine revert;

        public DragonSpecies Species => species;

        /// <summary>이번 결투의 드래곤으로 바꾸고 대기 그림을 보여 준다.</summary>
        public void SetSpecies(DragonSpecies newSpecies)
        {
            species = newSpecies;
            ShowIdle();
        }

        public void ShowIdle()
        {
            StopRevert();
            Apply(species != null ? species.BattleIdle : null);
        }

        public void ShowAttack() => ShowBriefly(species != null ? species.BattleAttack : null);

        public void ShowHurt() => ShowBriefly(species != null ? species.BattleHurt : null);

        /// <summary>어두운 색으로 바로 바꾼다 (true) / 원래 색으로 바로 바꾼다 (false).</summary>
        public void SetDarkened(bool darkened)
        {
            if (image != null) image.color = darkened ? darkTint : Color.white;
        }

        /// <summary>어두운 색에서 원래 색으로 서서히 돌아온다.</summary>
        public IEnumerator FadeToNormal(float duration)
        {
            if (image == null) yield break;
            for (float t = 0f; t < duration; t += Time.deltaTime)
            {
                image.color = Color.Lerp(darkTint, Color.white, t / duration);
                yield return null;
            }
            image.color = Color.white;
        }

        public void PlayCry()
        {
            if (audioSource != null && species != null && species.Cry != null)
                audioSource.PlayOneShot(species.Cry);
        }

        void OnDisable() => StopRevert();

        void ShowBriefly(Sprite sprite)
        {
            StopRevert();
            Apply(sprite);
            if (isActiveAndEnabled) revert = StartCoroutine(RevertLater());
        }

        IEnumerator RevertLater()
        {
            yield return new WaitForSeconds(poseDuration);
            revert = null;
            Apply(species != null ? species.BattleIdle : null);
        }

        void StopRevert()
        {
            if (revert == null) return;
            StopCoroutine(revert);
            revert = null;
        }

        void Apply(Sprite sprite)
        {
            if (image == null) return;
            image.sprite = sprite;
            image.enabled = sprite != null;
        }
    }
}
