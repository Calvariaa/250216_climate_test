

#[compute]

#version 450

layout(local_size_x=32,local_size_y=32,local_size_z=1)in;

layout(set=0,binding=0,std430)buffer LocalTemp{
    float data[];
}local_temp;

layout(set=0,binding=1,std430)buffer NeighborIndex{
    uvec4 data[];
}neighbor_index;

layout(set=0,binding=2,std430)buffer GetTime{
    float last_time[];
    float time[];
}calc_time;

const float alpha=1e-4;
const uint length=128;

// float dTdt[length][length]={0};
float* ComputeHeatEquation(uint id,float delta,float uk[length],float uk_delta)
{
    float d2Tdx2=0;
    float d2Tdy2=0;
    
    d2Tdx2+=local_temp.data[neighbor_index.data[id].x]+uk[neighbor_index.data[id].x]*uk_delta;
    d2Tdx2+=local_temp.data[neighbor_index.data[id].y]+uk[neighbor_index.data[id].y]*uk_delta;
    d2Tdy2+=local_temp.data[neighbor_index.data[id].z]+uk[neighbor_index.data[id].z]*uk_delta;
    d2Tdy2+=local_temp.data[neighbor_index.data[id].w]+uk[neighbor_index.data[id].w]*uk_delta;
    
    d2Tdx2-=2*(local_temp.data[id]+(uk[id]*uk_delta));
    d2Tdy2-=2*(local_temp.data[id]+(uk[id]*uk_delta));
    
    dTdt[id]=alpha*(d2Tdx2*dx2+d2Tdy2*dx2);// 将矩阵展平成向量
    return dTdt; 
}

void rk4(uint id,float[]cells,float dt,float dx2,float alpha)
{
    float* k1=ComputeHeatEquation(cells,null,0,dx2,alpha);
    var k2=ComputeHeatEquation(cells,k1,(float)delta/2.f,dx2,alpha);
    var k3=ComputeHeatEquation(cells,k2,(float)delta/2.f,dx2,alpha);
    var k4=ComputeHeatEquation(cells,k3,(float)delta,dx2,alpha);
    
    local_temp.data[id]+=dt/6*(k1[x,y]+2*k2[x,y]+2*k3[x,y]+k4[x,y]);
}

void main()
{
    // TODO
    calc_time.time[id]=timer.GetTime();

    uint x=gl_GlobalInvocationID.x;
    uint y=gl_GlobalInvocationID.y;
    uint z=gl_GlobalInvocationID.z;
    
    uint id=gl_GlobalInvocationID.x+
    gl_GlobalInvocationID.y*(gl_NumWorkGroups.x*gl_WorkGroupSize.x)+
    gl_GlobalInvocationID.z*(gl_NumWorkGroups.x*gl_WorkGroupSize.x)*
    (gl_NumWorkGroups.y*gl_WorkGroupSize.y);
    
    float delta=calc_time.time[id]-calc_time.last_time[id];

    
    
    calc_time.last_time[id]=calc_time.time[id];
}
