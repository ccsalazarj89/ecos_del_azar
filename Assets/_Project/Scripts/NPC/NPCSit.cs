using UnityEngine;

namespace EcosDelAzar.NPC
{
    /// <summary>
    /// Pone a un NPC en la pose de "sentado" a través del parámetro bool
    /// "IsSitting" de su Animator Controller. Busca el Animator también en
    /// los hijos, ya que en los NPCs basados en modelos Synty (p. ej. el
    /// Dealer) el componente Animator vive en el modelo anidado, no en la
    /// raíz del prefab.
    /// </summary>
    public class NPCSit : MonoBehaviour
    {
        static readonly int IsSittingHash = Animator.StringToHash("IsSitting");

        [SerializeField] Animator animator;
        [SerializeField] bool startSitting = true;
        [SerializeField] bool randomizeStartTime = true;

        void Awake()
        {
            if (animator == null) animator = GetComponentInChildren<Animator>();
        }

        void Start()
        {
            SetSitting(startSitting);

            // Desincroniza el bucle de animación entre NPCs que comparten el
            // mismo controller, para que no se muevan todos a la vez.
            if (randomizeStartTime && animator != null)
            {
                var state = animator.GetCurrentAnimatorStateInfo(0);
                animator.Play(state.fullPathHash, 0, Random.value);
            }
        }

        public void SetSitting(bool sitting)
        {
            if (animator == null) return;
            animator.SetBool(IsSittingHash, sitting);
        }
    }
}
