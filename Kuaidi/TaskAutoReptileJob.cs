using Quartz;
using System.Threading.Tasks;

namespace Kuaidi
{
    public class TaskAutoReptileJob : IJob
    {
        public Task Execute(IJobExecutionContext context)
        {
            return Task.Factory.StartNew(() =>
            {
                StartJob();
            });
        }

        private void StartJob()
        {
            //这里执行数据库订单号查询  忽略不写了
            Framain.framainDel("4304926503813");
        }

    }
}