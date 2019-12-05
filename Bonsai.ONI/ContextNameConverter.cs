using Bonsai.Expressions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.ONI
{
    class ContextNameConverter : StringConverter
    {
        public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
        {
            return true;
        }

        public override TypeConverter.StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
        {
            if (context != null)
            {
                var workflowBuilder = (WorkflowBuilder)context.GetService(typeof(WorkflowBuilder));
                if (workflowBuilder != null)
                {
                    var ctxNames = (from builder in workflowBuilder.Workflow.Descendants()
                                     let createCtx = ExpressionBuilder.GetWorkflowElement(builder) as CreateONIContext
                                     where createCtx != null && !string.IsNullOrEmpty(createCtx.Name)
                                     select !string.IsNullOrEmpty(createCtx.Name) ? createCtx.Name : createCtx.Name)
                                     .Concat(ONIManager.LoadConfiguration().Select(configuration => configuration.ContextName))
                                     .Distinct()
                                     .ToList();

                    if (ctxNames.Count > 0) return new StandardValuesCollection(ctxNames);
                    else return new StandardValuesCollection(SerialPort.GetPortNames());
                }
            }

            return base.GetStandardValues(context);
        }
    }
}
