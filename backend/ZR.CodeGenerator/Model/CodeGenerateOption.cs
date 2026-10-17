namespace ZR.CodeGenerator.Model
{
    public class CodeGenerateOption
    {
        /// <summary>
        /// ??????
        /// </summary>
        public string BaseNamespace { get; set; }
        /// <summary>
        /// ??????
        /// </summary>
        public string SubNamespace { get; set; }
        /// <summary>
        /// ????????
        /// </summary>
        public string ModelsNamespace { get; set; }
        /// <summary>
        /// ????????????
        /// </summary>
        public string DtosNamespace { get; set; }
        ///// <summary>
        ///// ????????
        ///// </summary>
        //public string IRepositoriesNamespace { get; set; }
        /// <summary>
        /// ????????
        /// </summary>
        public string RepositoriesNamespace { get; set; }
        /// <summary>
        /// ????????
        /// </summary>
        public string IServicsNamespace { get; set; }
        /// <summary>
        /// ??????????
        /// </summary>
        public string ServicesNamespace { get; set; }

        /// <summary>
        /// Api???????
        /// </summary>
        public string ApiControllerNamespace { get; set; }
        /// <summary>
        /// ??????,?“,”??
        /// </summary>
        //public string TableList { get; set; }
    }
}

