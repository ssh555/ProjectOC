using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ML.Example.InventorySystem.CompositeSystem.UI
{
    public class FormulaItemContainer : MonoBehaviour
    {
        public PreviewPanel Owner;

        protected int[][] curFormula;

        protected FormulaItemBtn formulaItemTemplate;

        private void Awake()
        {
            this.formulaItemTemplate = this.transform.GetChild(0).GetComponent<FormulaItemBtn>();

        }

        public void RefreshFormulaContainer(int[][] formulas)
        {
            this.curFormula = formulas;
            if (curFormula == null)
            {
                this.gameObject.SetActive(false);
            }
            else
            {
                this.gameObject.SetActive(true);
                // Refresh
                foreach (Transform child in this.transform)
                {
                    // ²»É¾³ýÄ£°å
                    if (child == this.formulaItemTemplate.transform) continue;
                    Destroy(child.gameObject);
                }

                for (int i = 0; i < curFormula.Length; ++i)
                {
                    FormulaItemBtn rt = GameObject.Instantiate<FormulaItemBtn>(this.formulaItemTemplate, this.transform);
                    rt.Owner = this;
                    rt.ID = curFormula[i][0];
                    rt.Num = curFormula[i][1];
                    rt.gameObject.SetActive(true);
                }
            }
        }
    }
}

