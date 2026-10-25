"""Step 6 - Model selection: preprocessing pipelines + the candidate algorithms per task type.

Every candidate is a full scikit-learn Pipeline (preprocess -> estimator), so imputation,
encoding, scaling and TF-IDF are learned on the training folds only and travel with the
saved model into the API.

Candidates are deliberately varied - linear, tree, ensemble, kernel, neighbours, neural
network - plus a Dummy baseline. A model is only worth deploying if it beats the baseline.
Grids are small on purpose: the PMO tables hold tens of rows, not millions.
"""

from __future__ import annotations

import numpy as np
from sklearn.base import BaseEstimator, RegressorMixin
from sklearn.compose import ColumnTransformer
from sklearn.dummy import DummyClassifier, DummyRegressor
from sklearn.ensemble import (GradientBoostingClassifier, GradientBoostingRegressor,
                              RandomForestClassifier, RandomForestRegressor)
from sklearn.feature_extraction.text import TfidfVectorizer
from sklearn.impute import SimpleImputer
from sklearn.linear_model import LinearRegression, LogisticRegression, Ridge
from sklearn.neighbors import KNeighborsClassifier, KNeighborsRegressor
from sklearn.neural_network import MLPClassifier, MLPRegressor
from sklearn.pipeline import Pipeline
from sklearn.preprocessing import OneHotEncoder, StandardScaler
from sklearn.svm import SVC, SVR
from sklearn.tree import DecisionTreeClassifier

from .features import Dataset


def preprocessor(ds: Dataset) -> ColumnTransformer:
    parts = []
    if ds.numeric:
        # keep_empty_features: a column that is all-missing in one training run (e.g. no planned
        # estimates yet) stays in place, so the feature layout is stable between runs
        parts.append(("num", Pipeline([("impute", SimpleImputer(strategy="median", keep_empty_features=True)),
                                       ("scale", StandardScaler())]), ds.numeric))
    if ds.categorical:
        parts.append(("cat", Pipeline([("impute", SimpleImputer(strategy="most_frequent")),
                                       ("onehot", OneHotEncoder(handle_unknown="ignore"))]), ds.categorical))
    if ds.text:
        # a single column name (not a list) -> the vectorizer receives 1-D text
        parts.append(("txt", TfidfVectorizer(ngram_range=(1, 2), min_df=1, sublinear_tf=True), ds.text))
    return ColumnTransformer(parts, remainder="drop", sparse_threshold=0.0)


class LastValueRegressor(BaseEstimator, RegressorMixin):
    """Naive forecast: next month = last month (feature 'lag1'). The bar any model must beat."""

    def __init__(self, column: int = 0):
        self.column = column

    def fit(self, X, y):
        self.fallback_ = float(np.nanmean(y))
        return self

    def predict(self, X):
        X = np.asarray(X, dtype=float)
        v = X[:, self.column]
        return np.where(np.isnan(v), self.fallback_, v)


def classification_candidates(rs: int) -> dict[str, tuple[object, dict]]:
    """name -> (estimator, param grid keyed for the 'model' pipeline step)."""
    return {
        "baseline_majority": (DummyClassifier(strategy="most_frequent"), {}),
        "logistic_regression": (LogisticRegression(max_iter=2000, class_weight="balanced"),
                                {"model__C": [0.1, 1.0, 10.0]}),
        "decision_tree": (DecisionTreeClassifier(class_weight="balanced", random_state=rs),
                          {"model__max_depth": [2, 3, 5]}),
        "random_forest": (RandomForestClassifier(n_estimators=200, class_weight="balanced", random_state=rs),
                          {"model__max_depth": [3, None], "model__min_samples_leaf": [1, 3]}),
        "gradient_boosting": (GradientBoostingClassifier(random_state=rs),
                              {"model__n_estimators": [50, 150], "model__max_depth": [2, 3]}),
        "svm_rbf": (SVC(probability=True, class_weight="balanced", random_state=rs),
                    {"model__C": [0.5, 2.0], "model__gamma": ["scale"]}),
        "knn": (KNeighborsClassifier(), {"model__n_neighbors": [3, 5]}),
        "neural_net_mlp": (MLPClassifier(hidden_layer_sizes=(16,), max_iter=3000, random_state=rs),
                           {"model__alpha": [1e-3, 1e-1]}),
    }


def regression_candidates(rs: int, lag1_index: int) -> dict[str, tuple[object, dict]]:
    return {
        "baseline_mean": (DummyRegressor(strategy="mean"), {}),
        "baseline_last_month": (LastValueRegressor(column=lag1_index), {}),
        "linear_regression": (LinearRegression(), {}),
        "ridge": (Ridge(), {"model__alpha": [0.1, 1.0, 10.0]}),
        "random_forest": (RandomForestRegressor(n_estimators=300, random_state=rs),
                          {"model__max_depth": [3, None], "model__min_samples_leaf": [1, 3]}),
        "gradient_boosting": (GradientBoostingRegressor(random_state=rs),
                              {"model__n_estimators": [100, 200], "model__max_depth": [2, 3]}),
        "svr_rbf": (SVR(), {"model__C": [1e4, 1e5]}),
        "knn": (KNeighborsRegressor(), {"model__n_neighbors": [3, 5]}),
        "neural_net_mlp": (MLPRegressor(hidden_layer_sizes=(32,), max_iter=5000, random_state=rs),
                           {"model__alpha": [1e-2, 1.0]}),
    }


def build_pipeline(ds: Dataset, estimator) -> Pipeline:
    if isinstance(estimator, LastValueRegressor):
        # the naive model reads raw lag1, so skip scaling: impute only
        pre = ColumnTransformer([("num", SimpleImputer(strategy="median", keep_empty_features=True),
                                  ds.numeric)], remainder="drop")
        return Pipeline([("pre", pre), ("model", estimator)])
    return Pipeline([("pre", preprocessor(ds)), ("model", estimator)])
