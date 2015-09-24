using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Xml;
using FaultTreeAnalysis;
using System.Collections.Generic;
using System.IO;

namespace UnitTests
{
    [TestClass]
    public class ModelTests
    {
        [TestMethod]
        public void Model_nytt60()
        {
            Validation.AnalyseModel("../TestModels/nytt60-FMEAOutput", true);
        }

        [TestMethod]
        public void Model_Compound_Connection()
        {
            Validation.AnalyseModel("../TestModels/Compound_ConnectionFailureBehaviour-FMEAOutput", true);
        }

        [TestMethod]
        public void Model_FTA_ElectricSystem()
        {
            Validation.AnalyseModel("../TestModels/FTA_ElectricSystem_test_nocalc-FMEAOutput", true);
        }

        [TestMethod]
        public void Model_haita_AA()
        {
            Validation.AnalyseModel("../TestModels/haita_AA-FMEAOutput", true);
        }

        [TestMethod]
        public void Model_HazardModel()
        {
            Validation.AnalyseModel("../TestModels/HazardModel-FMEAOutput", true);
        }

        [TestMethod]
        public void Model_hondaFMEASizeTest()
        {
            Validation.AnalyseModel("../TestModels/hondaFMEASizeTest-FMEAOutput", true);
        }

        [TestMethod]
        public void Model_hondaFMEASizeTestNoNormal()
        {
            Validation.AnalyseModel("../TestModels/hondaFMEASizeTestNoNormal-FMEAOutput", true);
        }

        [TestMethod]
        public void Model_HydraulicsWithBranch()
        {
            Validation.AnalyseModel("../TestModels/HydraulicsWithBranch-FMEAOutput", true);
        }

        [TestMethod]
        public void Model_HydraulicsWithoutBranch()
        {
            Validation.AnalyseModel("../TestModels/HydraulicsWithoutBranch-FMEAOutput", true);
        }

        [TestMethod]
        public void Model_M140610ErichTest()
        {
            Validation.AnalyseModel("../TestModels/M140610ErichTest-FMEAOutput", true);
        }

        [TestMethod]
        public void Model_multi()
        {
            Validation.AnalyseModel("../TestModels/multi-FMEAOutput", true);
        }

        [TestMethod]
        public void Model_MultiPerspective()
        {
            Validation.AnalyseModel("../TestModels/MultiPerspective-FMEAOutput", true);
        }

        [TestMethod]
        public void Model_non_coherent()
        {
            Validation.AnalyseModel("../TestModels/non_coherent-FMEAOutput", false);
        }

        [TestMethod]
        public void Model_PerspectiveRiskTime()
        {
            Validation.AnalyseModel("../TestModels/PerspectiveRiskTime-FMEAOutput", true);
        }

        [TestMethod]
        public void Model_SignalSample()
        {
            Validation.AnalyseModel("../TestModels/SignalSample-FMEAOutput", true);
        }

        [TestMethod]
        public void Model_test_FTS_01_2compSeries_20090324()
        {
            Validation.AnalyseModel("../TestModels/test_FTS_01_2compSeries_20090324-FMEAOutput", true);
        }

        [TestMethod]
        public void Model_test_FTS_02_2compSeries_20090324()
        {
            Validation.AnalyseModel("../TestModels/test_FTS_02_2compSeries_20090324-FMEAOutput", true);
        }

        [TestMethod]
        public void Model_test_FTS_03_2compSeriesPCCF_20090324()
        {
            Validation.AnalyseModel("../TestModels/test_FTS_03_2compSeriesPCCF_20090324-FMEAOutput", true);
        }

        [TestMethod]
        public void Model_test_FTS_04_2compSeriesACCF_20090324()
        {
            Validation.AnalyseModel("../TestModels/test_FTS_04_2compSeriesACCF_20090324-FMEAOutput", true);
        }

        [TestMethod]
        public void Model_test_FTS_05_3compSeries_20090324()
        {
            Validation.AnalyseModel("../TestModels/test_FTS_05_3compSeries_20090324-FMEAOutput", true);
        }

        [TestMethod]
        public void Model_test_FTS_06_3compAND_20090324()
        {
            Validation.AnalyseModel("../TestModels/test_FTS_06_3compAND_20090324-FMEAOutput", true);
        }

        [TestMethod]
        public void Model_test_FTS_07_3compAND_20090324()
        {
            Validation.AnalyseModel("../TestModels/test_FTS_07_3compAND_20090324-FMEAOutput", true);
        }

        [TestMethod]
        public void Model_test_FTS_08_2compCircle_20090324()
        {
            Validation.AnalyseModel("../TestModels/test_FTS_08_2compCircle_20090324-FMEAOutput", true);
        }

        [TestMethod]
        public void Model_test_FTS_09_2compCircleLocalAND_20090325()
        {
            Validation.AnalyseModel("../TestModels/test_FTS_09_2compCircleLocalAND_20090325-FMEAOutput", true);
        }

        [TestMethod]
        public void Model_test_FTS_10_2MEs2Pumps_20090325()
        {
            Validation.AnalyseModel("../TestModels/test_FTS_10_2MEs2Pumps_20090325-FMEAOutput", true);
        }

        [TestMethod]
        public void Model_test_FTS_11_2MEs2Pumps_20090325()
        {
            Validation.AnalyseModel("../TestModels/test_FTS_11_2MEs2Pumps_20090325-FMEAOutput", true);
        }

        [TestMethod]
        public void Model_test_FTS_12_1compCalcModes_20090325()
        {
            Validation.AnalyseModel("../TestModels/test_FTS_12_1compCalcModes_20090325-FMEAOutput", true);
        }

        [TestMethod]
        public void Model_test_FTS_13_2OutDeviations1BasicEvent__20090326()
        {
            Validation.AnalyseModel("../TestModels/test_FTS_13_2OutDeviations1BasicEvent__20090326-FMEAOutput", true);
        }

        [TestMethod]
        public void Model_test_FTS_14_connectionTypes__20090326()
        {
            Validation.AnalyseModel("../TestModels/test_FTS_14_connectionTypes__20090326-FMEAOutput", true);
        }

        [TestMethod]
        public void Model_test_FTS_15_2comp2implementations_20090326()
        {
            Validation.AnalyseModel("../TestModels/test_FTS_15_2comp2implementations_20090326-FMEAOutput", true);
        }

        [TestMethod]
        public void Model_test_FTS_16_compound_20090326()
        {
            Validation.AnalyseModel("../TestModels/test_FTS_16_compound_20090326-FMEAOutput", true);
        }

        [TestMethod]
        public void Model_YorkExample()
        {
            Validation.AnalyseModel("../TestModels/YorkExample-FMEAOutput", true);
        }
    }
}
