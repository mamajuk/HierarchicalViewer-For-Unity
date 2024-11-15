# HierarchicalViewer-For-Unity

## Overview
<table><tr><td>
<img src="https://github.com/mamajuk/HierarchicalViewer-For-Unity/blob/main/readmy_gif.gif?raw=true">
</td></tr></table> 

```HierachicalViewer```는 선택한 ```GameObject```의 계층구조를 표시하고, 자식들의 ```Transform```을 직관적으로 수정하기 위해 개발된 컴포넌트입니다. **유니티 엔진**에서 메시를 씬에 배치하면, 해당 메시를 구성하는 본들의 계층구조와 동일한 ```GameObject```가 생성됩니다. 뷰포트에서는 ```GameObject```의 계층구조가 자세히 표시되지 않기 때문에, 특정 본의 트랜스폼을 변경하고 싶다면 **Hierarchy** 창에서 일일이 찾아야하는 번거로움이 있습니다. 

계층구조의 최상단에 위치하는 ```GameObject```에 ```HierachicalViewer``` 컴포넌트를 부착하고, 뷰포트에서 선택하면 해당 객체의 모든 계층구조가 노드가 표시됩니다. 이렇게 표시된 노드들 중 하나를 클릭하고 **W(이동)**, **E(회전)**, **R(크기)** 와 같은 단축키를 눌러 직관적으로 트랜스폼을 수정할 수 있습니다.
